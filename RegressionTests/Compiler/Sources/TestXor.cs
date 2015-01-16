using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestXor : TestCase
    {
        private long[] state = new long[1];
        private long[] block = new long[] {0x55};
        private long[] K = new long[] {0x11};
        private long[] hash = new long[]{0x33};

        public void test1()
        {
            state[0] = block[0] ^ (K[0] = hash[0]);

            AssertTrue(state[0] == 0x66);
        }

        public void test2()
        {
            K[0] = hash[0];
            state[0] = block[0] ^ K[0];

            AssertTrue(state[0] == 0x66);
        }
    }
}
