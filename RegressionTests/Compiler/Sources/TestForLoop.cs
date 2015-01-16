using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestForLoop : TestCase
    {
        public void testSimple1()
        {
            var j = 1;
            for (var i = 0; i < 10; i++)
            {
                AssertEquals(i + 1, j);
                j++;
            }
        }

        public void testSimple2()
        {
            var j = 1;
            for (var i = 0; i < 10; i += 2)
            {
                AssertEquals(i + 1, j);
                j++;
                j++;
            }
        }

        public void testSimple3()
        {
            var j = 1;
            for (var i = 0; i < /*Count*/(10); i++)
            {
                j++;
            }
            for (var i = 0; i < /*Count*/(5); i++)
            {
                AssertEquals(i + 11, j);
                j++;
            }
        }

        public int Count(int i)
        {
            return i;
        }

    }
}
