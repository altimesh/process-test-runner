using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleTestProject
{
    [TestClass]
    public class UnitTest1
    {
        bool zou;

        [TestCleanup]
        public void Cleanup()
        {
            zou = false;
        }

        [TestInitialize]
        public void Initialize()
        {
            zou = true;
        }

        [TestMethod]
        public void Initialized_Test()
        {
            Assert.IsTrue(zou);
        }

        [TestMethod]
        [Timeout(1000)]
        public void Timeout_Test()
        {
            while (true) { }
            Assert.Fail("timeout not reached");
        }

        [TestMethod]
        public void Abort_Test()
        {
            Environment.Exit(6); // abort
        }


        [TestMethod]
        public void Exit_Test()
        {
            Environment.Exit(0); // abort
        }

        [TestMethod]
        public void Fail_Test()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void Equality_Fail_Test()
        {
            Assert.AreEqual<int>(2, 3, "custom message");
        }

        [TestMethod]
        public void Inconclusive_Test()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Pass_Test()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Pass_Test_2()
        {
            Assert.IsTrue(true);
        }
    }
}
