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
            context.Users.Add(new Entities.User { IdEntity = 1, NameEntity = "NAME1", RoleEntity = new Role { IdEntity = 1, NameEntity = "ROLE" } });
            context.SaveChanges();
            var service = new UserService(context);
            var result = service.Get(x => x.IdModel == 1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ServiceTest_GetComplex()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new Entities.User { IdEntity = 1, NameEntity = "NAME1", RoleEntity = new Role { IdEntity = 1, NameEntity = "ROLE1" } });
            context.Users.Add(new Entities.User { IdEntity = 2, NameEntity = "NAME2", RoleEntity = new Role { IdEntity = 2, NameEntity = "ROLE2" } });
            context.Users.Add(new Entities.User { IdEntity = 3, NameEntity = "NAME3", RoleEntity = new Role { IdEntity = 3, NameEntity = "ROLE3" } });
            context.SaveChanges();
            var service = new UserService(context);
            var ids = new List<int> { 1, 2 };
            var result = service.Get(x => ids.Contains(x.IdModel.Value));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ServiceTest_Update()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new User { IdEntity = 1, NameEntity = "NAME1", RoleEntity = new Role { IdEntity = 1, NameEntity = "ROLE1" } });
            context.SaveChanges();
            var service = new UserService(context);
            service.Update(new UserModel { IdModel = 1, NameModel = "Modified", RoleModel = "ROLE2" });
            var updated = context.Users.Single(x => x.IdEntity == 1);
            var expected = new User { IdEntity = 1, NameEntity = "Modified", RoleEntity = new Role { NameEntity = "ROLE2" } };
            Assert.AreEqual(expected.IdEntity, updated.IdEntity);
            Assert.AreEqual(expected.NameEntity, updated.NameEntity);
            Assert.AreEqual(expected.RoleEntity.NameEntity, updated.RoleEntity.NameEntity);
        }

        [TestMethod]
        public void ServiceTest_Create()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            var service = new UserService(context);
            service.Create(new Models.UserModel { IdModel = 1, NameModel = "Created", RoleModel = "ROLE" });
            var created = context.Users.Single(x => x.IdEntity == 1);
            var expected = new User { IdEntity = 1, NameEntity = "Created" };
            var expectedRole = new Role { NameEntity = "ROLE" };
            Assert.AreEqual(expected.IdEntity, created.IdEntity);
            Assert.AreEqual(expected.NameEntity, created.NameEntity);
            Assert.AreEqual(expectedRole.NameEntity, created.RoleEntity.NameEntity);
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
                new UserModel { IdModel = 2, NameModel = "Created2", RoleModel = "ROLE1" }
            };
            service.Create(models);
            var createds = context.Users.ToList();
            var expecteds = new List<User>
            {
                new User { IdEntity = 1, NameEntity = "Created1" },
                new User { IdEntity = 2, NameEntity = "Created2", RoleEntity = new Role{NameEntity = "ROLE1" } }
            };
            foreach (var created in createds)
            {
                var expected = expecteds.Single(x => x.IdEntity == created.IdEntity);
                Assert.AreEqual(expected.IdEntity, created.IdEntity);
                Assert.AreEqual(expected.NameEntity, created.NameEntity);
                if (expected.RoleEntity is null)
                {
                    Assert.IsNull(created.RoleEntity);
                }
                else
                {
                    Assert.AreEqual(expected.RoleEntity.NameEntity, created.RoleEntity.NameEntity);
                }
            }
        }

        [TestMethod]
        public void ServiceTest_Delete()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            var entities = new List<User>
            {
                new User { IdEntity = 1, NameEntity = "Created1", RoleEntity = new Role{IdEntity = 1, NameEntity = "ROLE1" }},
                new User { IdEntity = 2, NameEntity = "Created2", RoleEntity = new Role{IdEntity = 2, NameEntity = "ROLE2" }}
            };
            context.Users.AddRange(entities);
            context.SaveChanges();
            var service = new UserService(context);
            service.Delete(x => x.IdModel == 1 || x.IdModel == 2);
            var actuals = context.Users.ToList();
            var expected = new List<User>();
            Assert.AreEqual(expected.Count, actuals.Count);
        }
    }
}
