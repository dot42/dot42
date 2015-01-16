using System;
using Dot42;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestUInt : TestCase
    {
        [Include]
        public const UInt32 SIGN_MASK = ((UInt32)1 << 31);

        public void testAssigmentHighBitOff()
        {
            uint i = 0x3fcdab89;
            AssertTrue(i == 0x3fcdab89);
        }

        public void testAssigmentHighBitOn()
        {
            uint i = 0xefcdab89;
            AssertTrue(i == 0xefcdab89);
        }

        public void _testSignMask()
        {
            AssertTrue(IsGreaterThanZero(SIGN_MASK));
        }

        public static bool IsGreaterThanZero(uint value)
        {
            return value > 0;
        }
    }
}
