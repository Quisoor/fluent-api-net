using FluentApiNet.Domain;
using FluentApiNet.Domain.Visitor.Aggregation;
using FluentApiNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentApiNet.Core
{
    /// <summary>
    /// Aggregation service base
    /// </summary>
    public abstract class AggregationServiceBase<TModel, TJoinResult> : IAggregationServiceBase<TModel>
        where TModel : class
        where TJoinResult : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationServiceBase"/> class.
        /// </summary>
        protected AggregationServiceBase()
        {
            Mappings = new List<MappingAggregator>();
        }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        public List<MappingAggregator> Mappings { get; set; }

        /// <summary>
        /// Joins the query.
        /// </summary>
        /// <returns></returns>
        protected abstract IQueryable<TJoinResult> JoinQuery();

        /// <summary>
        /// Orders the query if none order by is passed by operations.
        /// </summary>
        /// <returns></returns>
        protected abstract IOrderedQueryable<TJoinResult> OrderQuery(IQueryable<TJoinResult> query);

        /// <summary>
        /// Haves the clause.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        protected bool HaveClause(Expression<Func<TModel, dynamic>> member)
        {
            var mapping = Mappings
                .Single(x => x.ModelMember.Member.Name == (member.Body as MemberExpression).Member.Name);
            if (mapping.AttachedWhere == null)
            {
                return false;
            }
            if (mapping.AttachedWhere.Body is ConstantExpression)
            {
                var constant = mapping.AttachedWhere.Body as ConstantExpression;
                if (constant.Value is bool)
                {
                    return !(bool)constant.Value;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Retrieves the query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        protected IQueryable<TEntity> RetrieveQuery<TEntity>(Expression<Func<TModel, dynamic>> member)
        {
            var mapping = Mappings.Single(x => x.ModelMember.Member.Name == (member.Body as MemberExpression).Member.Name);
            return mapping.AttachedQuery.Cast<TEntity>();
        }

        /// <summary>
        /// Gets the specified operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Operations<TModel> operations, int? page, int? pageSize)
        {
            var visitor = new AggregationWhereVisitor<TModel>(Mappings.Cast<Mapping>().ToList());
            var results = new Results<TModel>();
            Mappings = Mappings.OrderBy(x => x.IsPrimaryKey).ToList();

            #region WHERE
            var where = operations.Where;
            foreach (var map in Mappings)
            {
                Expression whereExpression = null;
                if (where != null)
                {
                    var paramType = map.ModelMember.Type;
                    if (TypeTools.IsList(paramType))
                    {
                        paramType = TypeTools.GetListType(paramType);
                    }
                    var param = Expression.Parameter(paramType, where.Parameters.Single().Name);
                    whereExpression = visitor.VisitLambda(where, param);
                }
                var service = GetType().GetProperty(map.EntityMember.Member.Name).GetValue(this);
                var getQueryMethod = service.GetType().GetMethods().Single(x => x.Name == ServiceBase.GetQueryMethodName && x.GetParameters().Any());
                var getQueryMethodWithoutParameter = service.GetType().GetMethods().Single(x => x.Name == ServiceBase.GetQueryMethodName && !x.GetParameters().Any());
                if (whereExpression != null)
                {
                    var queryTemp = getQueryMethod.Invoke(service, new[] { whereExpression });
                    map.AttachedQuery = (IQueryable)queryTemp;
                }
                else
                {
                    var queryTemp = getQueryMethodWithoutParameter.Invoke(service, null);
                    map.AttachedQuery = (IQueryable)queryTemp;
                }
                map.AttachedService = service as ServiceBase;
                map.AttachedWhere = whereExpression as LambdaExpression;
            }
            #endregion

            var query = JoinQuery();

            #region ORDER BY

            query = ApplyOrderBy(query, operations);

            #endregion

            #region PAGINATION
            results.Count = query.Count();

            page = PaginationTools.LimitPage(page);
            pageSize = PaginationTools.LimitPageSize(pageSize);

            query = query.Skip((page.Value - 1) * pageSize.Value);
            query = query.Take(pageSize.Value);

            #endregion

            #region SELECT

            var queryParameter = Expression.Parameter(query.ElementType, query.ElementType.Name);
            var ctorGlobal = Expression.New(typeof(TModel));
            var assignmentsGlobal = new List<MemberAssignment>();
            foreach (var map in Mappings)
            {
                var propertyGlobal = typeof(TModel).GetProperty(map.ModelMember.Member.Name);
                var service = GetType().GetProperty(map.EntityMember.Member.Name).GetValue(this);
                var serviceMappings = service.GetType().GetProperty(nameof(ServiceBase.Mappings)).GetValue(service) as List<Mapping>;
                var propertyOfQueryType = Expression.MakeMemberAccess(queryParameter, query.ElementType.GetProperty(map.ModelMember.Member.Name));
                var serviceModelType = map.ModelMember.Type;
                var serviceEntityType = map.AttachedQuery.ElementType;
                Expression subQueryParameter = null;
                if (TypeTools.IsList(map.ModelMember.Type))
                {
                    serviceModelType = TypeTools.GetListType(serviceModelType);
                    subQueryParameter = Expression.Parameter(map.AttachedQuery.ElementType, map.AttachedQuery.ElementType.Name);
                }
                else
                {
                    subQueryParameter = propertyOfQueryType;
                }

                var ctor = Expression.New(serviceModelType);
                var assignments = new List<MemberAssignment>();
                foreach (var serviceMap in serviceMappings)
                {
                    var property = serviceModelType.GetProperty(serviceMap.ModelMember.Member.Name);
                    var propertyTarget = serviceEntityType.GetProperty(serviceMap.EntityMember.Member.Name);
                    var propertyTargetExp = Expression.MakeMemberAccess(subQueryParameter, propertyTarget);
                    assignments.Add(Expression.Bind(property, propertyTargetExp));
                }

                var init = Expression.MemberInit(ctor, assignments.ToArray());

                if (TypeTools.IsList(map.ModelMember.Type))
                {
                    var selectMethod = typeof(System.Linq.Enumerable)
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Single(x => x.Name == "Select" &&
                               x.GetParameters()[1].ParameterType.GetGenericArguments().Count() == 2)
                        .MakeGenericMethod(map.AttachedQuery.ElementType, serviceModelType);
                    var lambdaSelect = Expression.Lambda(init, subQueryParameter as ParameterExpression);
                    var callSelect = Expression.Call(null, selectMethod, propertyOfQueryType, lambdaSelect);
                    var binding = Expression.Bind(propertyGlobal, callSelect);
                    assignmentsGlobal.Add(binding);
                }
                else
                {
                    assignmentsGlobal.Add(Expression.Bind(propertyGlobal, init));
                }
            }

            var initGlobal = Expression.MemberInit(ctorGlobal, assignmentsGlobal.ToArray());
            var select = Expression.Lambda<Func<TJoinResult, TModel>>(initGlobal, queryParameter);
            results.Result = query.Select(select).ToList();
            #endregion

            return results;
        }

        /// <summary>
        /// Applies the order by.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="operations">The operations.</param>
        /// <returns></returns>
        private IOrderedQueryable<TJoinResult> ApplyOrderBy(IQueryable<TJoinResult> query, Operations<TModel> operations)
        {
            if (operations.OrderBy.Any() && operations.OrderBy.All(x => x.Body is MemberExpression))
            {
                foreach (var orderBy in operations.OrderBy)
                {
                    var propertyName = ((orderBy.Body as MemberExpression).Expression as MemberExpression).Member.Name;
                    var propertyServiceName = (orderBy.Body as MemberExpression).Member.Name;
                    var lambda = GetOrderByExpression(Mappings.Single(x => x.ModelMember.Member.Name == propertyName), propertyServiceName);
                    if (orderBy == operations.OrderBy.First())
                    {
                        query = Queryable.OrderBy(query, lambda);
                    }
                    else
                    {
                        query = Queryable.ThenBy(query, lambda);
                    }
                }
                return (IOrderedQueryable<TJoinResult>)query;
            }
            query = OrderQuery(query);
            return (IOrderedQueryable<TJoinResult>)query;
        }

        /// <summary>
        /// Gets the order by expression.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="propertyServiceName">Name of the property service.</param>
        /// <returns></returns>
        private dynamic GetOrderByExpression(MappingAggregator map, string propertyServiceName = null)
        {
            var service = GetType().GetProperty(map.EntityMember.Member.Name).GetValue(this);
            var serviceMappings = service.GetType().GetProperty(nameof(ServiceBase.Mappings)).GetValue(service) as List<Mapping>;
            var serviceMapping = serviceMappings.First();
            if (!string.IsNullOrEmpty(propertyServiceName))
            {
                serviceMapping = serviceMappings.Single(x => x.ModelMember.Member.Name == propertyServiceName);
            }
            var propertyService = map.AttachedQuery.ElementType.GetProperty(serviceMapping.EntityMember.Member.Name);
            var parameter = Expression.Parameter(typeof(TJoinResult));
            var leftMember = Expression.MakeMemberAccess(parameter, typeof(TJoinResult).GetProperty(map.ModelMember.Member.Name));
            var rightMember = Expression.MakeMemberAccess(leftMember, propertyService);
            return (dynamic)Expression.Lambda(rightMember, parameter);
        }
    }
}
