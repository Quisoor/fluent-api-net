using FluentApiNet.Test.Database;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace FluentApiNet.Test.UT.Services
{
    [TestClass]
    public class ServiceTest
    {
        [TestMethod]
        public void ServiceTest_Get()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new Entities.User { IdEntity = 1, NameEntity = "Aurélien" });
            context.SaveChanges();
            var service = new UserService(context);
            var result = service.Get(x => x.IdModel == 1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ServiceTest_Update()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new Entities.User { IdEntity = 1, NameEntity = "Aurélien" });
            context.SaveChanges();
            var service = new UserService(context);
            service.Update(new Models.UserModel { IdModel = 1, NameModel = "Modified" });
            var updated = context.Users.Single(x => x.IdEntity == 1);
            var expected = new User { IdEntity = 1, NameEntity = "Modified" };
            Assert.AreEqual(expected.IdEntity, updated.IdEntity);
            Assert.AreEqual(expected.NameEntity, updated.NameEntity);
        }
    }
}
