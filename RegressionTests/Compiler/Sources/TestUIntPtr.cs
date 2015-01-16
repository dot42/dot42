using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestUIntPtr : TestCase
    {
        private uint[] p = new uint[] {1, 2, 3, 4};

        private uint G1(uint x, uint y, uint z)
        {
            return default(uint);
        }

        private static uint Dim(uint x, uint y)
        {
            return default(uint);
        }

        public void test1()
        {
            uint j = 0;
            p[j] += G1(p[Dim(j, 3)], p[Dim(j, 10)], p[Dim(j, 511)]);
        }
    }
}
