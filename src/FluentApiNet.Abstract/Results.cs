using System.Collections.Generic;

namespace FluentApiNet.Abstract
{
    /// <summary>
    /// Results class
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    public class Results<TModel>
        where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Results{TModel}"/> class.
        /// </summary>
        public Results()
        {
            this.Result = new List<TModel>();
        }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public List<TModel> Result { get; set; }
    }
}
