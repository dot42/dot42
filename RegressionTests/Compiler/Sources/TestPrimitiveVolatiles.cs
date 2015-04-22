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
    }
}
