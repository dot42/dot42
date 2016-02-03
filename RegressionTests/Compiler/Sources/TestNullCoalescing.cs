using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestNullCoalescing : TestCase
    {
        private int count;

        public void testNullCoalescing1()
        {
            count = 0;
            var x = GetNull() ?? IncreaseCount();
            AssertEquals(1, count);
        }

        public void testNullCoalescing2()
        {
            count = 0;
            var x = GetNonNull() ?? IncreaseCount();
            AssertEquals(0, count);
        }

        public object GetNull()
        {
            return null;
        }

        public object GetNonNull()
        {
            return new object();;
        }

        public object IncreaseCount()
        {
            count += 1;
            return new object();
        }

    }
}
