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
            SelectMapping.Add(Mapping.Init<UserModel, User>(x => x.IdUser, x => x.Id));
            SelectMapping.Add(Mapping.Init<UserModel, User>(x => x.UserName, x => x.Name));
        }
    }
}
