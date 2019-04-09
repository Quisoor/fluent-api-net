using FluentApiNet.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentApiNet.Test.UT.Tools
{
    [TestClass]
    public class PaginationToolsTest
    {
        [TestMethod]
        public void LimitPage_Null()
        {
            var actual = PaginationTools.LimitPage(null);
            var expected = 1;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LimitPage_Zero()
        {
            var actual = PaginationTools.LimitPage(0);
            var expected = 1;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LimitPageSize_Null()
        {
            var actual = PaginationTools.LimitPageSize(null);
            int? expected = null;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void LimitPageSize_InferiorToZero()
        {
            var actual = PaginationTools.LimitPageSize(-1);
            int? expected = null;
            Assert.AreEqual(expected, actual);
        }
    }
}
