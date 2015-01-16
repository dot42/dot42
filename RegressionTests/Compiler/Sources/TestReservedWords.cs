using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestReservedWords : TestCase
    {
        public void testAbstract()
        {
            int @abstract = 10;

            AssertEquals(@abstract, 10);
        }

        public void testLong()
        {
            long @long = 10;

            AssertEquals(@long, 10);
        }
    }
}
