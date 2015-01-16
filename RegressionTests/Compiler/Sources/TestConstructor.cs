using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestConstructor : TestCase
    {
        public class Inner
        {
            public int I { get; private set; }

            public Inner(uint i)
                :this((int)i)
            {
            }

            public Inner(int i)
            {
                I = i;
            }
        }

        public void _test1()
        {
            var innerInt = new Inner(7);
            var innerUInt = new Inner( (uint)42);

            AssertEquals(7, innerInt.I);
            AssertEquals(42, innerUInt.I);
        }
    }
}
