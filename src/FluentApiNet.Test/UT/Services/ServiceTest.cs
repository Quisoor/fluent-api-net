using FluentApiNet.Test.Database;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Models;
using FluentApiNet.Test.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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

        [TestMethod]
        public void ServiceTest_Create()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            var service = new UserService(context);
            service.Create(new Models.UserModel { IdModel = 1, NameModel = "Created" });
            var created = context.Users.Single(x => x.IdEntity == 1);
            var expected = new User { IdEntity = 1, NameEntity = "Created" };
            Assert.AreEqual(expected.IdEntity, created.IdEntity);
            Assert.AreEqual(expected.NameEntity, created.NameEntity);
        }

        [TestMethod]
        public void ServiceTest_Create_Multiple()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            var service = new UserService(context);
            var models = new List<UserModel>
            {
                new UserModel { IdModel = 1, NameModel = "Created1" },
                new UserModel { IdModel = 2, NameModel = "Created2" }
            };
            service.Create(models);
            var createds = context.Users.ToList();
            var expecteds = new List<User>
            {
                new User { IdEntity = 1, NameEntity = "Created1" },
                new User { IdEntity = 2, NameEntity = "Created2" }
            };
            foreach (var created in createds)
            {
                var expected = expecteds.Single(x => x.IdEntity == created.IdEntity);
                Assert.AreEqual(expected.IdEntity, created.IdEntity);
                Assert.AreEqual(expected.NameEntity, created.NameEntity);
            }
        }
    }
}
