using FluentApiNet.Domain;
using FluentApiNet.Domain.Visitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FluentApiNet.Core
{
    /// <summary>
    /// Service base interface
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IServiceBase<TModel, TEntity>
        where TModel : class
        where TEntity : class
    {
        /// <summary>
        /// Gets the translator.
        /// </summary>
        /// <value>
        /// The translator.
        /// </value>
        TranslationVisitor<Func<TEntity, bool>> Translator { get; }

        /// <summary>
        /// Creates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        Results<TModel> Create(TModel model);

        /// <summary>
        /// Creates the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns></returns>
        Results<TModel> Create(IEnumerable<TModel> models);

        /// <summary>
        /// Deletes the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        bool Delete(Expression<Func<TModel, bool>> filters);

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        Results<TModel> Get(Expression<Func<TModel, bool>> filters);

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page);

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page, int? pageSize);

        /// <summary>
        /// Gets the specified operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <returns></returns>
        Results<TModel> Get(Operations<TModel> operations);

        /// <summary>
        /// Gets the specified operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        Results<TModel> Get(Operations<TModel> operations, int? page);

        /// <summary>
        /// Gets the specified operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        Results<TModel> Get(Operations<TModel> operations, int? page, int? pageSize);

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        IQueryable<TEntity> GetQuery(Expression<Func<TModel, bool>> filters);

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> GetQuery();

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        Results<TModel> Update(TModel model);

        /// <summary>
        /// Updates the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns></returns>
        Results<TModel> Update(IEnumerable<TModel> models);
    }
}
