using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestAddressOf : TestCase
    {
        private int intInstanceField = 700;
        private static int intStaticField = 400;

        public void testInt1()
        {
            int i = 5;
            AssertTrue(i.ToString() == "5");
        }

        public void testInt2()
        {
            AssertTrue(intInstanceField.ToString() == "700");
        }

        public void testInt3()
        {
            AssertTrue(intStaticField.ToString() == "400");
        }

        public void testInt4()
        {
            AssertTrue(ToString(8) == "8");
        }

        private string ToString(int x)
        {
            return x.ToString();
        }
    }
}
