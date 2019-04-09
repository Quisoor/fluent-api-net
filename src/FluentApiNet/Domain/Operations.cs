using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentApiNet.Domain
{
    public class Operations<TModel>
        where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Operations"/> class.
        /// </summary>
        public Operations()
        {
            Selects = new List<Expression<Func<TModel, dynamic>>>();
            OrderBy = new List<Expression<Func<TModel, dynamic>>>();
        }

        /// <summary>
        /// Gets or sets the where.
        /// </summary>
        /// <value>
        /// The where.
        /// </value>
        public Expression<Func<TModel, bool>> Where { get; set; }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>
        /// The order by.
        /// </value>
        public List<Expression<Func<TModel, dynamic>>> OrderBy { get; set; }

        /// <summary>
        /// Gets or sets the select.
        /// </summary>
        /// <value>
        /// The select.
        /// </value>
        public List<Expression<Func<TModel, dynamic>>> Selects { get; }
    }
}
