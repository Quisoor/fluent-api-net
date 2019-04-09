using System.Collections.Generic;

namespace FluentApiNet.Test.Models
{
    /// <summary>
    /// Aggregation model class
    /// </summary>
    public class AggregationModel
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public UserModel User { get; set; }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public IEnumerable<LocationModel> Locations { get; set; }
    }
}
