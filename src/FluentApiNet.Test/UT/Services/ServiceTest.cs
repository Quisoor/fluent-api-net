using FluentApiNet.Domain;
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
            context.Users.Add(new Entities.User { Id = 1, Name = "NAME1", Role = new Role { Id = 1, Name = "ROLE" } });
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
            context.Users.Add(new Entities.User { Id = 1, Name = "NAME1", Role = new Role { Id = 1, Name = "ROLE1" } });
            context.Users.Add(new Entities.User { Id = 2, Name = "NAME2", Role = new Role { Id = 2, Name = "ROLE2" } });
            context.Users.Add(new Entities.User { Id = 3, Name = "NAME3", Role = new Role { Id = 3, Name = "ROLE3" } });
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
            context.Users.Add(new User { Id = 1, Name = "NAME1", Role = new Role { Id = 1, Name = "ROLE1" } });
            context.SaveChanges();
            var service = new UserService(context);
            service.Update(new UserModel { IdModel = 1, NameModel = "Modified", RoleModel = "ROLE2" });
            var updated = context.Users.Single(x => x.Id == 1);
            var expected = new User { Id = 1, Name = "Modified", Role = new Role { Name = "ROLE2" } };
            Assert.AreEqual(expected.Id, updated.Id);
            Assert.AreEqual(expected.Name, updated.Name);
            Assert.AreEqual(expected.Role.Name, updated.Role.Name);
        }

        [TestMethod]
        public void ServiceTest_Create()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            var service = new UserService(context);
            service.Create(new Models.UserModel { IdModel = 1, NameModel = "Created", RoleModel = "ROLE" });
            var created = context.Users.Single(x => x.Id == 1);
            var expected = new User { Id = 1, Name = "Created" };
            var expectedRole = new Role { Name = "ROLE" };
            Assert.AreEqual(expected.Id, created.Id);
            Assert.AreEqual(expected.Name, created.Name);
            Assert.AreEqual(expectedRole.Name, created.Role.Name);
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
                new User { Id = 1, Name = "Created1" },
                new User { Id = 2, Name = "Created2", Role = new Role{Name = "ROLE1" } }
            };
            foreach (var created in createds)
            {
                var expected = expecteds.Single(x => x.Id == created.Id);
                Assert.AreEqual(expected.Id, created.Id);
                Assert.AreEqual(expected.Name, created.Name);
                if (expected.Role is null)
                {
                    Assert.IsNull(created.Role);
                }
                else
                {
                    Assert.AreEqual(expected.Role.Name, created.Role.Name);
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
                new User { Id = 1, Name = "Created1", Role = new Role{Id = 1, Name = "ROLE1" }},
                new User { Id = 2, Name = "Created2", Role = new Role{Id = 2, Name = "ROLE2" }}
            };
            context.Users.AddRange(entities);
            context.SaveChanges();
            var service = new UserService(context);
            service.Delete(x => x.IdModel == 1 || x.IdModel == 2);
            var actuals = context.Users.ToList();
            var expected = new List<User>();
            Assert.AreEqual(expected.Count, actuals.Count);
        }

        [TestMethod]
        public void ServiceTest_Aggregation_Get()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new User { Id = 1, Name = "NAME1", Role = new Role { Id = 1, Name = "ROLE1" } });
            context.Users.Add(new User { Id = 2, Name = "NAME2", Role = new Role { Id = 2, Name = "ROLE2" } });
            context.Users.Add(new User { Id = 3, Name = "NAME3", Role = new Role { Id = 3, Name = "ROLE3" } });
            context.Locations.Add(new Location { Id = 1, Name = "PARIS", UserId = 1 });
            context.Locations.Add(new Location { Id = 2, Name = "MADRID", UserId = 2 });
            context.Locations.Add(new Location { Id = 3, Name = "LONDON", UserId = 1 });
            context.SaveChanges();
            var userService = new UserService(context);
            var locationService = new LocationService(context);
            var service = new AggregationService(locationService, userService);
            var operations = new Operations<AggregationModel>
            {
                Where = x => x.Locations.Any(y => y.Name == "PARIS")
            };
            var result = service.Get(operations, null, null);
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void ServiceTest_Aggregation_Get_WithoutCondition()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new User { Id = 1, Name = "NAME1", Role = new Role { Id = 1, Name = "ROLE1" } });
            context.Users.Add(new User { Id = 2, Name = "NAME2", Role = new Role { Id = 2, Name = "ROLE2" } });
            context.Users.Add(new User { Id = 3, Name = "NAME3", Role = new Role { Id = 3, Name = "ROLE3" } });
            context.Locations.Add(new Location { Id = 1, Name = "PARIS", UserId = 1 });
            context.Locations.Add(new Location { Id = 2, Name = "MADRID", UserId = 2 });
            context.Locations.Add(new Location { Id = 3, Name = "LONDON", UserId = 1 });
            context.SaveChanges();
            var userService = new UserService(context);
            var locationService = new LocationService(context);
            var service = new AggregationService(locationService, userService);
            var operations = new Operations<AggregationModel>();
            var result = service.Get(operations, null, null);
            Assert.AreEqual(3, result.Count);
            Assert.IsNotNull(result);
        }
    }
}
