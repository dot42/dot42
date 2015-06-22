using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestPrimitiveVolatiles : TestCase
    {
        class X
        {
            public volatile int I;

            public X()
            {
                I = 1;
            }
        }

        public void test()
        {
            AssertEquals(1, new X().I);
        }

        public void testBooleanHashCode()
        {
            AssertEquals(true.GetHashCode(), ((object)true).GetHashCode());
            AssertEquals(false.GetHashCode(), ((object)false).GetHashCode());
        }

        public void testCharHashCode()
        {
            AssertEquals('a'.GetHashCode(), ((object)'a').GetHashCode());
            AssertEquals('b'.GetHashCode(), ((object)'b').GetHashCode());
            AssertEquals('\x1234'.GetHashCode(), ((object)'\x1234').GetHashCode());
            AssertEquals('\u1234'.GetHashCode(), ((object)'\u1234').GetHashCode());
        }

    }
}
