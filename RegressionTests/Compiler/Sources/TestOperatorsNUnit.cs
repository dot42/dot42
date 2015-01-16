using NUnit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    [TestFixture]
    public class TestOperatorsNUnit
    {
        private bool called = false;

        [SetUp]
        public void MySetup()
        {
            called = false;
        }

        #region OR

        [Test]
        public void TestOr1a()
        {
            if (GetFalse() || SetCalled(false))
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestOr2a()
        {
            if (GetFalse() || SetCalled(true))
            {
               //nothing to do
            }
            else
            {
                Assert.Fail(); 
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestOr3a()
        {
            if (GetTrue() || SetCalled(false))
            {
                //nothing to do
            }
            else
            {
                Assert.Fail();
            }

            Assert.IsFalse(called);
        }

        [Test]
        public void TestOr1b()
        {
            if (GetFalse() | SetCalled(false))
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestOr2b()
        {
            if (GetFalse() | SetCalled(true))
            {
                //nothing to do
            }
            else
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestOr3b()
        {
            if (GetTrue() | SetCalled(false))
            {
                //nothing to do
            }
            else
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        #endregion

        #region AND

        [Test]
        public void TestAnd1a()
        {
            if (GetFalse() && SetCalled(false))
            {
                Assert.Fail();
            }

            Assert.IsFalse(called);
        }

        [Test]
        public void TestAnd2a()
        {
            if (GetTrue() && SetCalled(false))
            {
                Assert.Fail();
            }
           
            Assert.IsTrue(called);
        }

        [Test]
        public void TestAnd3a()
        {
            if (GetTrue() && SetCalled(true))
            {
                //nothing to do
            }
            else
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestAnd1b()
        {
            if (GetFalse() & SetCalled(false))
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestAnd2b()
        {
            if (GetTrue() & SetCalled(false))
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void TestAnd3b()
        {
            if (GetTrue() & SetCalled(true))
            {
                //nothing to do
            }
            else
            {
                Assert.Fail();
            }

            Assert.IsTrue(called);
        }

        #endregion

        private bool SetCalled(bool returns)
        {
            called = true;
            return returns;
        }

        private bool GetTrue()
        {
            return true;
        }

        private bool GetFalse()
        {
            return false;
        }
    }
}
