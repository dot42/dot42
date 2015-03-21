using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestArrayEnumerable : TestCase
    {
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

        public void testForEachFromCall()
        {
            int sum = 0;
            foreach (var c in GetIEnumerable())
                sum += (int)c;

            AssertEquals(15, sum);
        }

        private IEnumerable GetIEnumerable()
        {
            return new int[] { 1, 2, 3, 4, 5 };
        }

        public void testForEachGenericFromCall()
        {
            int sum = 0;
            foreach (var c in GetIEnumerableT())
                sum += c;

            AssertEquals(15, sum);
        }

        private IEnumerable<int> GetIEnumerableT()
        {
            return new int[] { 1, 2, 3, 4, 5 };
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
    }
}
