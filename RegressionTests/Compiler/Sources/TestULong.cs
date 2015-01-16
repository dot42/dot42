using Dot42;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestULong : TestCase
    {
        [Include]
        public const ulong SIGN_MASK = ((ulong)1L << 63);

        public void testAssigmentHighBitOff()
        {
            ulong i = 0x1122334455667788;
            AssertTrue(i == 0x1122334455667788);
        }

        public void testAssigmentHighBitOn()
        {
            ulong i = 0x8877665544332211;
            AssertTrue(i == 0x8877665544332211);
        }
    }
}
