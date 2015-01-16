using System;
using System.IO;
using Java.Lang;
using Junit.Framework;
using Exception = System.Exception;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestTryCatch : TestCase
    {
        private void Foo(int x)
        {
            
        }

        public void testX()
        {
            
        }
        
        public void testTryFinally()
        {
            try
            {
                Foo(0);
            }
            finally
            {
                Foo(25);
            }
        }

        public void testTryCatch()
        {
            try
            {
                Foo(0);
            }
            catch (Exception)
            {
                Foo(1);
            }
        }

        public void testTryCatchFinally()
        {
            try
            {
                Foo(0);
            }
            catch (Exception)
            {
                Foo(1);
            }
            finally
            {
                Foo(25);
            }
        }

        public void testTryCatchAllFinally()
        {
            try
            {
                Foo(0);
            }
            catch
            {
                Foo(2);
            }
            finally
            {
                Foo(25);
            }
        }

        public void testTryCatchCatchAllFinally()
        {
            try
            {
                Foo(0);
            }
            catch (Exception)
            {
                Foo(1);
            }
            catch
            {
                Foo(2);
            }
            finally
            {
                Foo(25);
            }
        }

        public void testNestedTryCatch1()
        {
            try
            {
                Foo(0);
                try
                {
                    Foo(100);
                }
                catch (SystemException)
                {
                    Foo(101);
                }
                Foo(10);
            }
            catch (Exception)
            {
                Foo(1);
            }
        }

        public void testNestedTryCatch2()
        {
            var outerThrown = false;
            var innerThrown = false;

            try
            {
                try
                {
                    throw new NullReferenceException();
                }
                catch
                {
                    innerThrown = true;
                }
            }
            catch (Exception)
            {
                outerThrown = true;
            }

            AssertTrue(innerThrown);
            AssertFalse(outerThrown);
        }

        public void testEmptyTryCatch()
        {
            try
            {
            }
            catch
            {
            }
        }

        public void testTryMultipleCatch1()
        {
            var thrown = false;
            try
            {
                throw new NullReferenceException();
            }
            catch(NullReferenceException)
            {
                thrown = true;
            }
            catch (Exception e)
            {
                AssertTrue("Incorrect exception thrown: " + e, false);
            }

            AssertTrue(thrown);
        }

        public void testTryMultipleCatch2()
        {
            var thrown = false;
            try
            {
                throw new InvalidCastException();
            }
            catch (ArgumentException)
            {
                AssertTrue(false);
            }
            catch (Exception)
            {
                thrown = true;
            }

            AssertTrue(thrown);
        }

        public void testTryMultipleCatch3()
        {
            var thrown = false;
            try
            {
                ThrowEx();
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            catch (Exception e)
            {
                AssertTrue("Incorrect exception thrown: " + e, false);
            }

            AssertTrue(thrown);
        }

        public void testTryCatchCtr()
        {
            int i = 10;
            i = 12;
            i = 134;
            i = 1234;
            i = 12345;

            var thrown = false;
            try
            {
                new MyTestClass("", true);
            }
            catch (ArgumentException e)
            {
                thrown = true;
            }
            catch (Exception e)
            {
                AssertTrue("Incorrect exception thrown: " + e, false);
            }

            AssertTrue(thrown);

            thrown = false;
            try
            {
                new MyTestClass("", true);
            }
            catch (ArgumentNullException)
            {
                AssertTrue(false);
            }
            catch (Exception)
            {
                thrown = true;
            }

            AssertTrue(thrown);

            thrown = false;
            try
            {
                new MyTestClass("", true);
            }
            catch (ArgumentOutOfRangeException e)
            {
                AssertTrue(false);
            }
            catch (Exception)
            {
                thrown = true;
            }

            AssertTrue(thrown);

            thrown = false;
            try
            {
                new MyTestClass("", true);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            catch (Exception e)
            {
                AssertTrue("Incorrect exception thrown: " + e, false);
            }

            AssertTrue(thrown);

            thrown = false;
            try
            {
                new MyTestClass("", true);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            catch (Exception e)
            {
                AssertTrue("Incorrect exception thrown: " + e, false);
            }

            AssertTrue(thrown);

            thrown = false;
            try
            {
                new MyTestClass("", true);
            }
            catch (ArgumentException)
            {
                thrown = true;
            }
            catch (Exception e)
            {
                AssertTrue("Incorrect exception thrown: " + e, false);
            }

            AssertTrue(thrown);

            i = 10;
            i = 12;
            i = 134;
            i = 1234;
            i = 12345;
        }

        private void ThrowEx()
        {
            throw new ArgumentException("This is a test");
        }

        private class MyTestClass
        {
            public MyTestClass(string x, bool y)
            {
                throw new ArgumentException("Test");
            }
        }
    }
}
