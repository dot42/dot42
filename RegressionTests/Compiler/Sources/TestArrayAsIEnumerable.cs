using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


        public void testInt1()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> enumerable = array;
            var sum = enumerable.Sum();
            AssertEquals(15, sum);
        }


        public void testInt2()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            var sum = MyStaticSum(array);
            AssertEquals(15, sum);
        }

        public void testInt3()
        {
            var array = new int[] { 1, 2, 3, 4 };
            var sum = MyInstanceSum(array);
            AssertEquals(10, sum);
        }

        public void testIntCount1()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            var count = MyStaticCount(array);
            AssertEquals(5, count);
        }

        public void testIntCount2()
        {
            var array = new int[] { 1, 2, 3, 4 };
            var count = MyInstanceCount(array);
            AssertEquals(4, count);
        }

        public void testForEachIntPlainIEnumerableExplicit()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            int sum = 0;
            var enumerable = (IEnumerable)array;
            foreach (var c in enumerable)
                sum += (int)c;

            AssertEquals(15, sum);
        }

        public void testForEachIntPlainIEnumerableImplicit()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            int sum = 0;
            foreach (var c in (IEnumerable)array)
                sum += (int)c;

            AssertEquals(15, sum);
        }

        public void testForEachIntGenericIEnumerableImplicit()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            int sum = 0;
            foreach (var c in (IEnumerable<int>)array)
                sum += c;

            AssertEquals(15, sum);
        }

        public void testForEachIntGenericIEnumerableExplicit()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            int sum = 0;
            var enumerable = (IEnumerable<int>)array;
            foreach (var c in enumerable)
                sum += c;

            AssertEquals(15, sum);
        }

        public void testForEachImplicitFromFromCallReturn()
        {
            int sum = 0;
            foreach (var c in GetIEnumerable())
                sum += (int)c;

            AssertEquals(15, sum);
        }

     

        public void testForEachGenericFromCall()
        {
            int sum = 0;
            foreach (var c in GetIEnumerableT())
                sum += c;

            AssertEquals(15, sum);
        }

        public void testGetEnumerator()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            int sum = 0;

            var x = array.GetEnumerator();

            while (x.MoveNext())
                sum += (int)x.Current;

            AssertEquals(15, sum);
        }

        public void testForEachInt()
        {
            var array = new int[] { 1, 2, 3, 4, 5 };
            int sum = 0;
            foreach (var c in array)
                sum += c;

            AssertEquals(15, sum);
        }

        private IEnumerable GetIEnumerable()
        {
            return new int[] { 1, 2, 3, 4, 5 };
        }

        private IEnumerable<int> GetIEnumerableT()
        {
            return new int[] { 1, 2, 3, 4, 5 };
        }


        private static int MyInstanceCount<T>(IEnumerable<T> enumerable)
        {
            return enumerable.Count();
        }

        private static int MyStaticCount<T>(IEnumerable<T> enumerable)
        {
            return enumerable.Count();
        }

        private static int MyInstanceSum(IEnumerable<int> enumerable)
        {
            return enumerable.Sum();
        }

        private static int MyStaticSum(IEnumerable<int> enumerable)
        {
            return enumerable.Sum();
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
