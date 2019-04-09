using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FluentApiNet.Test.Entities
{
    /// <summary>
    /// Role class entity
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>
        /// The users.
        /// </value>
        public virtual ICollection<User> Users { get; set; }
    }
}
