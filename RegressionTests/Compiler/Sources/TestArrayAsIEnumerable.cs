using System.Collections;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestArrayAsIEnumerable : TestCase
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
            AssertTrue(arr is IEnumerable);
        }

        public void testIs2()
        {
            var arr = new bool[] { true, false, true };
            AssertTrue(IsIEnumerable(arr));
        }

        public void testAs1()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = arr as IEnumerable;
            AssertNotNull(c);
        }

        public void testAs2()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = AsIEnumerable(arr);
            AssertNotNull(c);
        }

        public void testCast1()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = (IEnumerable)arr;
            AssertNotNull(c);
        }

        public void testCast2()
        {
            var arr = new int[] { 4, 5, 6 };
            var c = CastToIEnumerable(arr);
            AssertNotNull(c);
        }

        private static int Count(IEnumerable c)
        {
            var result = 0;
            foreach (var x in c)
            {
                result ++;
            }
            return result;
        }

        private static int Sum(IEnumerable c)
        {
            var result = 0;
            foreach (var x in c)
            {
                result += (int)x;
            }
            return result;
        }

        public static IEnumerable CastToIEnumerable(object x)
        {
            return (IEnumerable)x;
        }

        public static IEnumerable AsIEnumerable(object x)
        {
            return x as IEnumerable;
        }

        public static bool IsIEnumerable(object x)
        {
            return x is IEnumerable;
        }
    }
}
