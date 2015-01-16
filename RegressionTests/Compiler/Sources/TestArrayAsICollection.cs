using System;
using System.Collections;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestArrayAsICollection : TestCase
    {
        public void test1()
        {
            var arr = new int[] { 5, 10 };
            var sum = Sum(arr);
            AssertEquals(15, sum);
            AssertEquals(2, Count(arr));
        }

        public void testIs1()
        {
            var arr = new bool[] { true, false, true };
            AssertTrue(arr is ICollection);
        }

        public void testIs2()
        {
            var arr = new bool[] { true, false, true };
            AssertTrue(IsICollection(arr));
        }

        public void testAs1()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = arr as ICollection;
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        public void testAs2()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = AsICollection(arr);
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        public void testCast1()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = (ICollection)arr;
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        public void testCast2()
        {
            var arr = new int[] {4, 5, 6};
            var c = CastToICollection(arr);
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        private static int Count(ICollection c)
        {
            return c.Count;
        }

        private static int Sum(ICollection c)
        {
            var result = 0;
            foreach (var x in c)
            {
                result += (int)x;
            }
            return result;
        }

        public static ICollection CastToICollection(object x)
        {
            return (ICollection)x;
        }

        public static ICollection AsICollection(object x)
        {
            return x as ICollection;
        }

        public static bool IsICollection(object x)
        {
            return x is ICollection;
        }
    }
}
