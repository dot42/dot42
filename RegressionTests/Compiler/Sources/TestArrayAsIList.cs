using System.Collections;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestArrayAsIList : TestCase
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
            AssertTrue(arr is IList);
        }

        public void testIs2()
        {
            var arr = new bool[] { true, false, true };
            AssertTrue(IsIList(arr));
        }

        public void testAs1()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = arr as IList;
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        public void testAs2()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = AsIList(arr);
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        public void testCast1()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = (IList)arr;
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        public void testCast2()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = CastToIList(arr);
            AssertNotNull(c);
            AssertEquals(3, c.Count);
        }

        private static int Count(IList c)
        {
            return c.Count;
        }

        private static int Sum(IList c)
        {
            var result = 0;
            foreach (var x in c)
            {
                result += (int)x;
            }
            return result;
        }

        public static IList CastToIList(object x)
        {
            return (IList)x;
        }

        public static IList AsIList(object x)
        {
            return x as IList;
        }

        public static bool IsIList(object x)
        {
            return x is IList;
        }
    }
}
