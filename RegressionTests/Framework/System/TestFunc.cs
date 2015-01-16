using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestFunc : TestCase
    {
        private static string staticString;
        private int instanceInt;
        private long instanceLong;

        public void testStaticFunc()
        {
            var x = new Func<string, string>(TheStaticFunc);
            var result = x("hello");
            AssertEquals("hello-", result);
            AssertEquals("hello", staticString);
        }

        private static string TheStaticFunc(string x)
        {
            staticString = x;
            return x + "-";
        }

        public void testInstanceFunc1()
        {
            var x = new Func<int, int>(TheInstanceFunc);
            var result = x(6245);
            AssertEquals(6250, result);
            AssertEquals(6245, instanceInt);
        }

        private int TheInstanceFunc(int x)
        {
            instanceInt = x;
            return x + 5;
        }

        public void testInstanceFunc2()
        {
            var x = new Func<long, long>(TheInstanceFunc);
            var result = x(6245);
            AssertEquals(6255, result);
            AssertEquals(6245, instanceLong);
        }

        private long TheInstanceFunc(long x)
        {
            instanceLong = x;
            return x + 10;
        }
    }
}
