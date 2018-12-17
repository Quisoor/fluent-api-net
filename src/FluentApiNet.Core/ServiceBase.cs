using FluentApiNet.Domain;
using FluentApiNet.Domain.Visitor;
using FluentApiNet.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Z.EntityFramework.Plus;

namespace FluentApiNet.Core
{
    public class ServiceBase<TModel, TEntity, TContext>
        where TModel : class, new()
        where TEntity : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// The translator
        /// </summary>
        private readonly TranslationVisitor<Func<TEntity, bool>> translator;

        /// <summary>
        /// Gets or sets mapping.
        /// </summary>
        /// <value>
        /// The select mapping.
        /// </value>
        private readonly List<Mapping> mappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase{TModel, TEntity, TContext}"/> class.
        /// </summary>
        public ServiceBase()
        {
            mappings = new List<Mapping>();
            // initialize the translator
            translator = new TranslationVisitor<Func<TEntity, bool>>();
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        protected TContext Context { get; set; }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>
        /// The repository.
        /// </value>
        protected DbSet<TEntity> Repository
        {
            get
            {
                // get repository property in the context
                var repositoryProperty = typeof(TContext).GetProperties()
                    .Single(p => p.PropertyType.IsGenericType
                        && p.PropertyType.Name.StartsWith("DbSet")
                        && p.PropertyType.GetGenericArguments().Length > 0
                        && p.PropertyType.GetGenericArguments().First() == typeof(TEntity));
                // get value of the repository
                var repository = (DbSet<TEntity>)repositoryProperty.GetValue(Context);
                return repository;
            }
        }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>
        /// The order by.
        /// </value>
        protected MemberExpression OrderBy { get; set; }

        /// <summary>
        /// Adds the mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        protected void AddMapping(Mapping mapping)
        {
            mappings.Add(mapping);
            translator.AddMapping(mapping);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters)
        {
            return Get(filters, PaginationTools.DEFAULT_PAGE, PaginationTools.DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page)
        {
            return Get(filters, page, PaginationTools.DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page, int? pageSize)
        {
            var results = new Results<TModel>();

            // format pagination
            page = PaginationTools.LimitPage(page);
            pageSize = PaginationTools.LimitPageSize(pageSize);

            var query = GetQuery(filters);

            // get the count
            results.Count = query.Count();

            // apply pagination
            query = query.Pagine(page.Value, pageSize.Value);

            // get the results
            results.Result = ApplySelect(query);

            return results;
        }

        /// <summary>
        /// Creates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Created model</returns>
        public Results<TModel> Create(TModel model)
        {
            // map the entity
            var entity = new TEntity();
            entity = Map(ref entity, model);

            // add entity to database
            Repository.Add(entity);
            Context.SaveChanges();

            // format the result
            var results = new Results<TModel>
            {
                Count = 1
            };
            results.Result.Add(Map(ref model, entity));

            // return the result
            return results;
        }

        /// <summary>
        /// Creates the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns>Created models</returns>
        public Results<TModel> Create(IEnumerable<TModel> models)
        {
            // map entities
            var entities = new List<TEntity>();
            foreach (var model in models)
            {
                var entity = new TEntity();
                entity = Map(ref entity, model);
                entities.Add(entity);
            }
            // add entities to database
            Repository.AddRange(entities);
            Context.SaveChanges();

            // format results
            var results = new Results<TModel>
            {
                Count = models.Count(),
                Result = entities.Select(x =>
                {
                    var model = new TModel();
                    return Map(ref model, x);
                })
                .ToList()
            };

            // return results
            return results;
        }

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Updated model</returns>
        public Results<TModel> Update(TModel model)
        {
            // construct where expression with key properties
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            var keyMappings = mappings.Where(x => x.IsPrimaryKey).ToList();
            Expression expression = Expression.Constant(true);
            foreach (var keyMapping in keyMappings)
            {
                var left = Expression.MakeMemberAccess(parameter, keyMapping.EntityMember.Member);
                var keyProperty = typeof(TModel).GetProperty(keyMapping.ModelMember.Member.Name);
                var right = Expression.Constant(keyProperty.GetValue(model));
                var equal = Expression.Equal(left, right);
                expression = Expression.AndAlso(expression, equal);
            }

            // search entity
            var entity = Repository.Single(Expression.Lambda<Func<TEntity, bool>>(expression, parameter));

            // update entity
            Map(ref entity, model);
            Context.SaveChanges();

            // format results
            var results = new Results<TModel>
            {
                Count = 1
            };
            results.Result.Add(model);

            // return results
            return results;
        }

        /// <summary>
        /// Updates the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns>Updated models</returns>
        public Results<TModel> Update(IEnumerable<TModel> models)
        {
            var results = new Results<TModel>();
            foreach (var model in models)
            {
                var result = Update(model);
                results.Count = results.Count + result.Count;
                results.Result.AddRange(result.Result);
            }
            return results;
        }

        /// <summary>
        /// Deletes the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>If is deleted</returns>
        public bool Delete(Expression<Func<TModel, bool>> filters)
        {
            var query = GetQuery(filters);
            var result = query.Delete();
            Context.SaveChanges();
            return result > 0;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        protected IQueryable<TEntity> GetQuery(Expression<Func<TModel, bool>> filters)
        {
            // get basic query of the repository
            var query = GetQuery();
            // apply the where expression to the query
            query = ApplyWhere(query, filters);
            // apply order by expression to the query
            query = ApplyOrderBy(query);
            return query;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns>The repository</returns>
        private IQueryable<TEntity> GetQuery()
        {
            var repository = Repository;
            return repository;
        }

        /// <summary>
        /// Applies the where.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="filters">The filters.</param>
        /// <returns>Query filtered</returns>
        private IQueryable<TEntity> ApplyWhere(IQueryable<TEntity> query, Expression<Func<TModel, bool>> filters)
        {
            var where = translator.Visit(filters) as LambdaExpression;
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(where.Body, translator.EntryParameter));
            return query;
        }

        /// <summary>
        /// Applies the order by.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Query ordered</returns>
        private IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query)
        {
            // get the order by expression
            MemberExpression orderBy = null;
            if (OrderBy == null && mappings.Count >= 1)
            {
                orderBy = translator.Visit(mappings.First().ModelMember) as MemberExpression;
            }
            else if (OrderBy != null)
            {
                orderBy = translator.Visit(OrderBy) as MemberExpression;
            }

            // apply expressions
            if (orderBy != null)
            {
                var lambda = (dynamic)Expression.Lambda(orderBy, translator.EntryParameter);
                query = Queryable.OrderBy(query, lambda);
            }

            return query;
        }

        /// <summary>
        /// Applies the select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of results</returns>
        private List<TModel> ApplySelect(IQueryable<TEntity> query)
        {
            // new TModel()
            var ctor = Expression.New(typeof(TModel));
            var assignments = new List<MemberAssignment>();
            foreach (var map in mappings)
            {
                // add assignment for the model property
                var property = typeof(TModel).GetProperty(map.ModelMember.Member.Name);
                assignments.Add(Expression.Bind(property, translator.Visit(map.ModelMember)));
            }
            // initialize the model
            var init = Expression.MemberInit(ctor, assignments.ToArray());
            // create select expression
            var select = Expression.Lambda<Func<TEntity, TModel>>(init, translator.EntryParameter);
            // apply select expression
            return query.Select(select).ToList();
        }

        /// <summary>
        /// Maps the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="model">The model.</param>
        /// <returns>Mapped entity</returns>
        private TEntity Map(ref TEntity entity, TModel model)
        {
            foreach (var map in mappings)
            {
                var modelProperty = typeof(TModel).GetProperty(map.ModelMember.Member.Name);
                var modelValue = modelProperty.GetValue(model);
                if (modelValue != null)
                {
                    string entityPropertyName = map.EntityMember.Member.Name;
                    var entityExpression = map.EntityMember.Expression as Expression;
                    while (entityExpression is MemberExpression)
                    {
                        var expression = entityExpression as MemberExpression;
                        entityPropertyName = string.Concat(expression.Member.Name, ".", entityPropertyName);
                        entityExpression = expression.Expression;
                    }
                    SetProperty(entityPropertyName, entity, modelValue);
                }
            }
            return entity;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="compoundProperty">The compound property.</param>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        private void SetProperty(string compoundProperty, object target, object value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                if(propertyToGet.GetValue(target) == null && propertyToGet.PropertyType.IsClass)
                {
                    var propertyGetValue = Activator.CreateInstance(propertyToGet.PropertyType);
                    propertyToGet.SetValue(target, propertyGetValue);
                }
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }

        /// <summary>
        /// Maps the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>Mapped model</returns>
        private TModel Map(ref TModel model, TEntity entity)
        {
            foreach (var map in mappings)
            {
                var entityProperty = typeof(TEntity).GetProperty(map.EntityMember.Member.Name);
                var modelProperty = typeof(TModel).GetProperty(map.ModelMember.Member.Name);
                modelProperty.SetValue(model, entityProperty.GetValue(entity));
            }
            return model;
        }
    }
}
