using Dot42;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestULong : TestCase
    {
        private ulong d1 = 1;
        private readonly ulong d8 = 8;
        private static ulong d2 = 2;
        private static readonly ulong d3 = 3;

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

        public void testHashCode()
        {
            ulong l = 0x8100000020000000;
            ulong[] ll = { l };

            AssertEquals(d1.GetHashCode(), ((object)d1).GetHashCode());
            AssertEquals(d8.GetHashCode(), ((object)d8).GetHashCode());
            AssertEquals(d3.GetHashCode(), ((object)d3).GetHashCode());
            AssertEquals(l.GetHashCode(), ((object)l).GetHashCode());
            AssertEquals(ll[0].GetHashCode(), ((object)l).GetHashCode());
            AssertEquals(hashCodeByRef(ref l), ((object)l).GetHashCode());
            AssertEquals(hashCodeByRef(ref d1), ((object)d1).GetHashCode());
        }

        public int hashCodeByRef(ref ulong l)
        {
            return l.GetHashCode();
        }
    }
}
