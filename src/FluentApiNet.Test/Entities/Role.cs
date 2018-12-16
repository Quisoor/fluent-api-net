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
        /// Gets or sets the identifier entity.
        /// </summary>
        /// <value>
        /// The identifier entity.
        /// </value>
        [Key]
        public int IdEntity { get; set; }

        /// <summary>
        /// Gets or sets the name entity.
        /// </summary>
        /// <value>
        /// The name entity.
        /// </value>
        public string NameEntity { get; set; }

        /// <summary>
        /// Gets or sets the users entity.
        /// </summary>
        /// <value>
        /// The users entity.
        /// </value>
        public virtual ICollection<User> UsersEntity { get; set; }
    }
}
