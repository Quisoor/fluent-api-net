using FluentApiNet.Abstract;
using FluentApiNet.Core;
using FluentApiNet.Test.Database;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Models;
using System.Linq;

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
        public UserService(TestDbContext context)
        {
            this.Context = context;

            // mapping
            AddMapping(Mapping.Init<UserModel, User>(x => x.IdModel, x => x.IdEntity));
            AddMapping(Mapping.Init<UserModel, User>(x => x.NameModel, x => x.NameEntity));
        }

        public void Update(UserModel model)
        {
            var entity = GetQuery(x => x.IdModel == model.IdModel).Single();
            entity.NameEntity = model.NameModel;
            Context.SaveChanges();
        }
    }
}
