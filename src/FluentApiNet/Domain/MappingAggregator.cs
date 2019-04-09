using FluentApiNet.Core;
using FluentApiNet.Domain.Visitor;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Domain
{
    /// <summary>
    /// Mapping aggregator class
    /// </summary>
    public class MappingAggregator : Mapping
    {
        /// <summary>
        /// Gets or sets the attached service.
        /// </summary>
        /// <value>
        /// The attached service.
        /// </value>
        public ServiceBase AttachedService { get; set; }

        /// <summary>
        /// Gets or sets the attached query.
        /// </summary>
        /// <value>
        /// The attached query.
        /// </value>
        public IQueryable AttachedQuery { get; set; }

        /// <summary>
        /// Gets or sets the attached where.
        /// </summary>
        /// <value>
        /// The attached where.
        /// </value>
        public LambdaExpression AttachedWhere { get; set; }

        /// <summary>
        /// Initializes the specified model lambda.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="modelLambda">The model lambda.</param>
        /// <param name="entityLambda">The entity lambda.</param>
        /// <returns>Initialized mapping</returns>
        public static new MappingAggregator Init<TModel, TEntity>(Expression<Func<TModel, dynamic>> modelLambda, Expression<Func<TEntity, dynamic>> entityLambda)
        {
            return Init(modelLambda, entityLambda, false);
        }

        /// <summary>
        /// Initializes the specified model lambda.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="modelLambda">The model lambda.</param>
        /// <param name="entityLambda">The entity lambda.</param>
        /// <param name="isPrimaryKey">if set to <c>true</c> [is primary key].</param>
        /// <returns>Initialized mapping</returns>
        public static new MappingAggregator Init<TModel, TEntity>(Expression<Func<TModel, dynamic>> modelLambda, Expression<Func<TEntity, dynamic>> entityLambda, bool isPrimaryKey)
        {
            var mapping = new MappingAggregator();
            mapping.Set(modelLambda, entityLambda);
            mapping.IsPrimaryKey = isPrimaryKey;
            return mapping;
        }
    }
}
