using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestAction : TestCase
    {
        private static string staticString;
        private int instanceInt;
        private long instanceLong;

        public void testStaticAction1()
        {
            var x = new Action<string>(TheStaticAction);
            x("hello");
            AssertEquals("hello", staticString);
        }

        private static void TheStaticAction(string x)
        {
            staticString = x;
        }

        public void testInstanceAction1()
        {
            var x = new Action<int>(TheInstanceAction);
            x(6245);
            AssertEquals(6245, instanceInt);
        }

        private void TheInstanceAction(int x)
        {
            instanceInt = x;
        }

        public void testInstanceAction2()
        {
            var x = new Action<long>(TheInstanceAction);
            x(6245);
            AssertEquals(6245, instanceLong);
        }

        private void TheInstanceAction(long x)
        {
            instanceLong = x;
        }
    }
}
