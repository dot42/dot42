using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestChecked : TestCase
    {
        public void testIntUnderflow1()
        {
            int a = int.MinValue;
            AssertEquals(a, int.MinValue);

            bool exceptionThrown = false;

            try
            {
                checked
                {
                    a--;
                }
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown);
        }

        public void testIntUnderflow2()
        {
            int a = -2000000000;
            AssertEquals("a", a, -2000000000);

            bool exceptionThrown = false;

            try
            {
                checked
                {
                    a = a * 2000;
                }
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue("exception expected", exceptionThrown);
        }

        public void testIntOverflow1()
        {
            int a = int.MaxValue;
            AssertEquals(a, int.MaxValue);

            bool exceptionThrown = false;

            try
            {
                a = checked(a + 1);
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown);
        }

        public void testIntOverflow2()
        {
            int a = int.MaxValue;
            AssertEquals(a, int.MaxValue);

            bool exceptionThrown = false;

            try
            {
                a = checked(a * 2);
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown);
        }

        public void testLongUnderflow1()
        {
            long a = long.MinValue;
            AssertEquals(a, long.MinValue);

            bool exceptionThrown = false;

            try
            {
                checked
                {
                    a--;
                }
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown);
        }

        public void testLongUnderflow2()
        {
            long a = long.MinValue;
            AssertEquals("a", a, long.MinValue);

            bool exceptionThrown = false;

            try
            {
                checked
                {
                    a = a * 2000;
                }
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue("exception expected", exceptionThrown);
        }

        public void testLongOverflow1()
        {
            long a = long.MaxValue;
            AssertEquals(a, long.MaxValue);

            bool exceptionThrown = false;

            try
            {
                a = checked(a + 1L);
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown);
        }

        public void testLongOverflow2()
        {
            long a = long.MaxValue;
            AssertEquals(a, long.MaxValue);

            bool exceptionThrown = false;

            try
            {
                a = checked(a * 2L);
            }
            catch (OverflowException)
            {
                exceptionThrown = true;
            }

            AssertTrue(exceptionThrown);
        }
    }
}
