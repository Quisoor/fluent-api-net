using System;
using FluentApiNet.Test.Database;
using FluentApiNet.Test.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentApiNet.Test.UT
{
    [TestClass]
    public class ServiceTest
    {
        [TestMethod]
        public void ServiceTest_Basic()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();
            var context = new TestDbContext(connection);
            context.Users.Add(new Entities.User { Id = 1, Name = "Aurélien" });
            context.SaveChanges();
            var service = new UserService(context);
            var result = service.Get(x => x.IdUser == 1);
            Assert.IsNotNull(result);
        }
    }
}
