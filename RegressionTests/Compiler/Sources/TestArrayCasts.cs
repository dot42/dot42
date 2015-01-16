using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestArrayCasts : TestCase
    {
        public void test1()
        {
            object x = new object[5];
            AssertTrue(IsArray(x));
        }

        public void test2()
        {
            object x = new object();
            AssertFalse(IsArray(x));
        }

        public void test3()
        {
            object x = new object[5];
            AssertNotNull(AsArray(x));
        }

        public void test4()
        {
            object x = new object();
            AssertNull(AsArray(x));
        }

        public void test5()
        {
            var x = new ushort[1];
            var b = (byte) 5;

            x[0] = b;

            AssertEquals(5, x[0]);
        }

        public void test6()
        {
            var x = new ushort[1,1];
            var b = (byte)5;

            x[0,0] = b;

            AssertEquals(5, x[0,0]);
        }

        public void test7()
        {
            var x = new ushort[1, 1];
            var b = (byte)5;
            var s = (ushort) b;

            x[0, 0] = s;

            AssertEquals(5, x[0, 0]);
        }

        public static bool IsArray(object x)
        {
            return x is Array;
        }

        public static object AsArray(object x)
        {
            return x as Array;
        }
    }
}
