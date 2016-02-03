using System;
using Android.OS;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestTryCatch : TestCase
    {
        private int lastFoo;

        private void Foo(int x)
        {
            lastFoo = x;
        }

        public void testX()
        {
            
        }
        
        public void testTryFinally()
        {
            try
            {
                Foo(2);
            }
            finally
            {
                AssertEquals(2, lastFoo);
                Foo(25);
            }
            AssertEquals(25, lastFoo);
        }

        public void testTryCatch()
        {
            try
            {
                Foo(2);
            }
            catch (Exception)
            {
                Foo(1);
            }

            AssertEquals(2, lastFoo);
        }

        public void testTryCatchFinally()
        {
            try
            {
                Foo(2);
            }
            catch (Exception)
            {
                Foo(1);
            }
            finally
            {
                AssertEquals(2, lastFoo);
                Foo(25);
            }

            AssertEquals(25, lastFoo);
        }

        public void testTryCatchAllFinally()
        {
            try
            {
                Foo(1);
            }
            catch
            {
                Foo(2);
            }
            finally
            {
                AssertEquals(1, lastFoo);
                Foo(25);
            }

            AssertEquals(25, lastFoo);
        }

        public void testTryCatchCatchAllFinally()
        {
            try
            {
                Foo(3);
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
                AssertEquals(3, lastFoo);
                Foo(25);
            }

            AssertEquals(25, lastFoo);
        }

        public void testNestedTryCatch1()
        {
            try
            {
                Foo(2);
                try
                {
                    Foo(100);
                }
                catch (SystemException)
                {
                    Foo(101);
                }

                AssertEquals(100, lastFoo);
                Foo(10);
            }
            catch (Exception)
            {
                Foo(1);
            }

            AssertEquals(10, lastFoo);
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

        public void testNestedTryCatchRethrow()
        {
            var outerThrown = false;
            var middleThrown = false;
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
                    try
                    {
                        throw new ArgumentException();
                    }
                    catch (Exception)
                    {
                        middleThrown = true;
                    }
                    throw;
                }
            }
            catch (NullReferenceException)
            {
                outerThrown = true;
            }

            AssertTrue(innerThrown);
            AssertTrue(middleThrown);
            AssertTrue(outerThrown);
        }

        public void testNestedTryFinally()
        {
            int count = 0;
            try
            {
                count ++;
                try { count++; } 
                finally { count++; }
            }
            finally
            {
                count++;
                try { count++; }
                finally { count++; }
            }
            AssertEquals(6, count);
        }

        public void testNestedTryFinallyGoto()
        {
            int count = 0;
            var nan = double.NaN;
            try
            {
                count++;

                try
                {
                    if(double.IsNaN(nan))
                        goto assert;
                    count++;
                }
                finally { count++; }

            }
            finally
            {
                count++;
                try { count++; }
                finally { count++; }
            }

            count = 0;
            assert:

            AssertEquals(5, count);
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
