using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestPrimitiveArrays : TestCase
    {
        public void testAlloc()
        {
            var s = new char[4];
            AssertNotNull(s);
        }

        public void testLength()
        {
            var s = new char[4];
            AssertEquals(s.Length, 4);
        }

        public void testSetByte()
        {
            var s = new byte[342];
			s[10] = 5;
        }

        public void testGetByte1()
        {
            var s = new byte[4];
			s[1] = 34;
            AssertEquals((int)s[1], 34);
        }

        public void testGetByte2()
        {
            var s = new byte[4];
			s[3] = 200; // Above 127
            AssertEquals((int)s[3], 200);
        }

        public void testSetBool()
        {
            var s = new bool[342];
			s[10] = false;
        }

        public void testGetBool()
        {
            var s = new bool[4];
			s[1] = true;
            AssertEquals(s[1], true);
        }

        public void testSetChar()
        {
            var s = new char[4];
			s[0] = 'd';
        }

        public void testGetChar()
        {
            var s = new char[4];
			s[0] = 'd';
            AssertEquals(s[0], 'd');
        }

        public void testSetShort()
        {
            var s = new short[4];
			s[1] = 12000;
        }

        public void testGetShort()
        {
            var s = new short[34];
			s[33] = 5523;
            AssertEquals((short)s[33], (short)5523);
        }

        public void testSetUInt16()
        {
            var s = new ushort[4];
			s[1] = 12000;
        }

        public void testGetUInt16()
        {
            var s = new ushort[34];
			s[33] = 5523;
            AssertEquals(s[33], 5523);
        }

        public void testGetUInt16_2()
        {
            var s = new ushort[1];
            s[0] = 60000;
            AssertEquals(s[0], 60000);
        }

        public void testSetInt()
        {
            var s = new int[4];
			s[1] = 12000;
        }

        public void testGetInt()
        {
            var s = new int[34];
			s[33] = 5523;
            AssertEquals(s[33], 5523);
        }
	}
}
