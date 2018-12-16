﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentApiNet.Test.Entities
{
    /// <summary>
    /// User entity class
    /// </summary>
    public class User
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
        /// Gets or sets the identifier role entity.
        /// </summary>
        /// <value>
        /// The identifier role entity.
        /// </value>
        public int? RoleEntityId { get; set; }

        /// <summary>
        /// Gets or sets the role entity.
        /// </summary>
        /// <value>
        /// The role entity.
        /// </value>
        public Role RoleEntity { get; set; }
    }
}
