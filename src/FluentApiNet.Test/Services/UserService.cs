using System.Linq;
using FluentApiNet.Core;
using FluentApiNet.Domain;
using FluentApiNet.Test.Database;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Models;

namespace FluentApiNet.Test.Services
{
    /// <summary>
    /// User service class
    /// </summary>
    /// <seealso cref="FluentApiNet.Core.ServiceBase{FluentApiNet.Test.Models.UserModel, FluentApiNet.Test.Entities.User, FluentApiNet.Test.Database.TestDbContext}" />
    public class UserService : ServiceBase<UserModel, User, TestDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public UserService(TestDbContext context) : base()
        {
            Context = context;

            // mapping
            AddMapping(Mapping.Init<UserModel, User>(x => x.IdModel, x => x.Id, true));
            AddMapping(Mapping.Init<UserModel, User>(x => x.NameModel, x => x.Name));
            AddMapping(Mapping.Init<UserModel, User>(x => x.RoleModel, x => x.Role.Name));
        }

        /// <summary>
        /// Orders the query if none order by is passed by operations.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override IOrderedQueryable<User> OrderQuery(IQueryable<User> query)
        {
            return query.OrderBy(x => x.Id);
        }
    }
}
