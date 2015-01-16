using Java.Util;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestForEach : TestCase
    {
        public void testIntArray()
        {
            var arr = new[] { 1, 2, 3, 4, 5, 6 };
            var index = 0;
            foreach (var value in arr)
            {
                AssertEquals(index + 1, value);
                index++;
            }
            AssertEquals(index, 6);
        }

        public void testStringArray()
        {
            var arr = new[] { "a", "bb", "ccc", "dddd" };
            var index = 0;
            foreach (var value in arr)
            {
                AssertEquals(index + 1, value.Length);
                index++;
            }
            AssertEquals(index, 4);
        }

        public void testStringList()
        {
            var arr = new ArrayList<string>();
            arr.Add("a");
            arr.Add("bb");
            arr.Add("ccc");
            arr.Add("dddd");
            var index = 0;
            foreach (var value in arr)
            {
                AssertEquals(index + 1, value.Length);
                index++;
            }
            AssertEquals(index, 4);
        }
    }
}
