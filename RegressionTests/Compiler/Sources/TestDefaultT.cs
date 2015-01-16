using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestDefaultT : TestCase
    {
        public void testByte()
        {
            var x = default(byte);
            AssertEquals((int)x, 0);
        }

        public void testShort()
        {
            var x = default(short);
            AssertEquals(x, (short)0);
        }
        public void testInt()
        {
            var x = default(int);
            AssertEquals(x, 0);
        }

        public void testBool()
        {
            var x = default(bool);
            AssertEquals(x, false);
        }

        public void testChar()
        {
            var x = default(char);
            AssertEquals(x, '\0');
        }

        public void testFloat()
        {
            var x = default(float);
            AssertEquals(x, 0.0f);
        }

        public void testLong()
        {
            var x = default(long);
            AssertEquals(x, 0L);
        }

        public void testDouble()
        {
            var x = default(double);
            AssertEquals(x, 0.0, 0.000001);
        }

        public void testString()
        {
            var x = default(string);
            AssertEquals(x, null);
        }

        public void testObject()
        {
            var x = default(object);
            AssertEquals(x, null);
        }
    }
}
