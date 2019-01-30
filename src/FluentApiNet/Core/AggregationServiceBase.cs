﻿using FluentApiNet.Domain;
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
    public abstract class AggregationServiceBase<TModel, TJoinResult>
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
        /// Gets or sets the order by.
        /// </summary>
        /// <value>
        /// The order by.
        /// </value>
        protected MemberExpression OrderBy { get; set; }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        public List<MappingAggregator> Mappings { get; set; }

        protected abstract IQueryable<TJoinResult> JoinQuery();

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
            }
            #endregion

            var query = JoinQuery();

            #region PAGINATION
            results.Count = query.Count();

            page = page.HasValue ? page.Value : ServiceBase.DefaultPage;
            pageSize = pageSize.HasValue ? pageSize.Value <= ServiceBase.MaxPageSize ? pageSize.Value : ServiceBase.MaxPageSize : ServiceBase.DefaultPageSize;

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


    }
}
