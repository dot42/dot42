using Dot42;
using Android.Test.Suitebuilder;
using NUnit.Framework;
using System;

namespace Dot42.Tests
{
    [TestFixture]
    public class TearDownTestCase
    {
        private bool test1;
        private bool test2;
        private bool teardown;

        [TearDown]
        public void MyTearDown()
        {
            teardown = true;
            Assert.IsTrue(test1 | test2);
        }

        [Test]
        public void MyTest1()
        {
            test1 = true;
            Assert.IsTrue(test1 & test2 == teardown);
        }

        [Test]
        public void MyTest2()
        {
            test2 = true;
            Assert.IsTrue(test1 & test2 == teardown);
        }
    }
}
