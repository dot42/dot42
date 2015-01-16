using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestDecimal : TestCase
    {
        public void _test1()
        {
            decimal d1 = 19932143214312.32M;
            decimal d2 = -8995034512332157M;

            AssertTrue(d1 > d2);
        }
    }
}
