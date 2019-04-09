using FluentApiNet.Domain;
using System.Collections.Generic;

namespace FluentApiNet.Core
{
    /// <summary>
    /// Aggregation service base interface
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public interface IAggregationServiceBase<TModel>
        where TModel : class
    {
        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>
        /// The mappings.
        /// </value>
        List<MappingAggregator> Mappings { get; set; }

        /// <summary>
        /// Gets the specified operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        Results<TModel> Get(Operations<TModel> operations, int? page, int? pageSize);
    }
}
