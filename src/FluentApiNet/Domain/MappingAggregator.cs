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
        public Expression AttachedWhere { get; set; }
    }
}
