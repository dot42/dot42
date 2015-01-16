using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestString : TestCase
    {
        public void testLength()
        {
            var s = "Hello";
            AssertEquals(s.Length, 5);
        }

        public void testCharAt()
        {
            var s = "Hello";
            AssertEquals(s[0], 'H');
        }

        public void testIsNullOrEmpty1()
        {
            AssertEquals(string.IsNullOrEmpty(null), true);
        }

        public void testIsNullOrEmpty2()
        {
            AssertEquals(string.IsNullOrEmpty(""), true);
        }

        public void testIsNullOrEmpty3()
        {
            var s = "Hello";
            AssertEquals(string.IsNullOrEmpty(s), false);
        }

        public void testConcat1()
        {
            var s = "Hello";
            AssertEquals(s, "Hello");
            AssertEquals(s + "-there", "Hello-there");
        }

        public void testConcat2()
        {
            var s = "Hello";
            AssertEquals(s, "Hello");
            AssertEquals(s + 5, "Hello5");
        }

        public void testConcat3()
        {
            var s = "Hello";
            AssertEquals(s, "Hello");
            AssertEquals(s + 'a', "Helloa");
        }

        public void testEnumerable()
        {
            var s = "Hello";

            int i = 0;
            foreach (var c in s)
            {
                AssertEquals(s[i], c);
                i++;
            }
        }
    }
}
