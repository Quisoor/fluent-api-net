using FluentApiNet.Abstract;
using FluentApiNet.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Core
{
    public class ServiceBase<TModel, TEntity, TContext>
        where TModel : class, new()
        where TEntity : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase{TModel, TEntity, TContext}"/> class.
        /// </summary>
        public ServiceBase()
        {
            SelectMapping = new List<Mapping>();
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        protected TContext Context { get; set; }

        /// <summary>
        /// Gets or sets the select mapping.
        /// </summary>
        /// <value>
        /// The select mapping.
        /// </value>
        protected List<Mapping> SelectMapping { get; set; }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters)
        {
            return this.Get(filters, PaginationTools.DEFAULT_PAGE, PaginationTools.DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page)
        {
            return this.Get(filters, page, PaginationTools.DEFAULT_PAGESIZE);
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
            query = query.Skip(page.Value - 1 * pageSize.Value).Take(pageSize.Value);

            // get the results
            results.Result = QueryTools.Transpose<TModel, TEntity>(query, SelectMapping);

            return results;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        protected IQueryable<TEntity> GetQuery(Expression<Func<TModel, bool>> filters)
        {
            var translator = new TranslationVisitor<Func<TEntity, bool>>(SelectMapping);
            var query = GetQuery();
            var where = translator.Visit(filters) as LambdaExpression;
            query = query.Where(Expression.Lambda<Func<TEntity,bool>>(where.Body, Expression.Parameter(typeof(TEntity),"x")));
            return query;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<TEntity> GetQuery()
        {
            var entityType = typeof(TEntity);
            var contextProperties = typeof(TContext).GetProperties();
            var repositoryProperty = typeof(TContext).GetProperties()
                .Single(p => p.PropertyType.IsGenericType
                    && p.PropertyType.Name.StartsWith("DbSet")
                    && p.PropertyType.GetGenericArguments().Length > 0
                    && p.PropertyType.GetGenericArguments().First() == typeof(TEntity));
            var repository = (DbSet<TEntity>)repositoryProperty.GetValue(Context);
            return repository;
        }
    }
}
