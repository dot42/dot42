using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestOperators : TestCase
    {
        public void testAssigment()
        {
            int t;

            t = X[0] = S[0];

            AssertTrue(t == 41);
            AssertTrue(X[0] == 41);
            AssertTrue(S[0] == 41);
        }

        public void testXor()
        {
            int t;

            t = X[0] ^= S[0];

            AssertTrue(t == 56);
            AssertTrue(X[0] == 56);
            AssertTrue(S[0] == 41);
        }

        private static readonly byte[] S = {(byte) 41};
        private byte[] X = {(byte) 17};

        public void testRem1()
        {
            uint a = 6;
            uint b = 4;

            a %= b;

            AssertTrue(a == 2);
        }

        public void testLeftShift()
        {
            uint lsw = 3;
            int i = 2;
            if ((lsw & (1 << i)) != 0)
            {
                Fail();
            }
        }

        public void testLeftShift2()
        {
            uint buffer = 0x78;

            byte first = 0x9C;
            byte second = 0x2D;

            buffer |= (uint)((first & 0xff | ((second & 0xff) << 8)) << 8);

            AssertEquals(0x2D9C78, buffer);
        }
    }
}
