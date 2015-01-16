using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestString : TestCase
    {
        public void testFormat1()
        {
			int i = 25;
            var x = string.Format("{0}", i);
            AssertEquals("25", x);
        }

        public void testFormat2()
        {
            int i = 25;
            var x = string.Format("\n{0}\n", i);
            AssertEquals("\n25\n", x);
        }

        public void testFormatDouble1()
        {
			double i = 25.2;
            var x = string.Format("{0:0.00}", i);
            AssertEquals("25.20", x);
        }

        public void testContains()
        {
            var s = "hello world";
            AssertTrue(s.Contains("hello"));
            AssertFalse(s.Contains("nothello"));
        }

        public void testIndexOfAny()
        {
            const string path = "nonexistentfile";
            AssertTrue(path.IndexOfAny(new[] {' '}) == -1);
            AssertFalse(path.IndexOfAny(new[] { 'f' }) == -1);
        }

        public void testStaticEquals()
        {
            AssertTrue("#1", string.Equals("abc", "abc"));
            AssertFalse("#2", string.Equals("abc", "abcd"));
            
            AssertTrue("#3", string.Equals(null, null));
            AssertFalse("$4", string.Equals(null, "abc"));
        }
    }
}
