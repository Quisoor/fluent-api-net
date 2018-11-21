using System;
using FluentApiNet.Test.Database;
using FluentApiNet.Test.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentApiNet.Test.UT.services
{
    [TestClass]
    public class ServiceTest
    {
        [TestMethod]
        public void ServiceTest_Basic()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new Entities.User { IdEntity = 1, NameEntity = "Aurélien" });
            context.SaveChanges();
            var service = new UserService(context);
            var result = service.Get(x => x.IdModel == 1);
            Assert.IsNotNull(result);
        }
    }
}
