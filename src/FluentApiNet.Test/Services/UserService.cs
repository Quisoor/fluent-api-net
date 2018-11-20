using FluentApiNet.Abstract;
using FluentApiNet.Core;
using FluentApiNet.Test.Database;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Models;

namespace FluentApiNet.Test.Services
{
    public class UserService : ServiceBase<UserModel, User, TestDbContext>
    {
        public UserService(TestDbContext context)
        {
            this.Context = context;
            SelectMapping.Add(Mapping.Init<UserModel, User>(x => x.IdModel, x => x.IdEntity));
            SelectMapping.Add(Mapping.Init<UserModel, User>(x => x.NameModel, x => x.NameEntity));
        }
    }
}
