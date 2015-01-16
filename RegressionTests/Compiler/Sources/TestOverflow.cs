using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestOverflow : TestCase
    {
        public void testIntegralUnderflow()
        {
            int a = int.MinValue;
            AssertEquals(a, int.MinValue);

            a--;
            AssertEquals(a, int.MaxValue);
        }

        public void testIntegralOverflow()
        {
            int a = int.MaxValue;
            AssertEquals(a, int.MaxValue);

            a++;
            AssertEquals(a, int.MinValue);
        }

       
    }
}
