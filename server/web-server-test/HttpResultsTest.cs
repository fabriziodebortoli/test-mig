using System;
using Microarea.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace web_server_test
{
    [TestClass]
    public class HttpResultsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            SuccessResult obj = new SuccessResult();
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestMethod2()
        {
            SuccessResult obj = new SuccessResult { Content = "pippo" };
            Assert.IsTrue(obj.Content == "pippo");
        }

        [TestMethod]
        public void TestMethod3()
        {
            SuccessResult obj = new SuccessResult ("pippo");
            Assert.IsTrue(obj.StatusCode == 200);
        }
    }
}
