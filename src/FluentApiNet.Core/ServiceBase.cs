using FluentApiNet.Abstract;
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
            return this.Get(filters, 1, 25);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel,bool>> filters, int? page, int? pageSize)
        {
            var results = new Results<TModel>();

            return results;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        protected IQueryable<TEntity> GetQuery(Expression<Func<TModel, bool>> filters)
        {
            var query = GetQuery();
            return query;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<TEntity> GetQuery()
        {
            var repositoryProperty = typeof(TContext).GetProperties().Single(x => x.PropertyType.GenericTypeArguments.First() == typeof(TEntity));
            var repository = (DbSet<TEntity>)repositoryProperty.GetValue(Context);
            return repository;
        }

        /// <summary>
        /// Transposes the filter.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        private Expression<Func<TEntity, bool>> TransposeFilter(Expression<Func<TModel, bool>> filters)
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            return null;
        }
    }
}
