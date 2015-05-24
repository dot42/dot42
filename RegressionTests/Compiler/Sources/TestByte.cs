using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestByte : TestCase
    {
        [Include]
        public const byte SIGN_MASK = ((byte)1 << 7);

		private byte instanceVar;
		private static byte staticVar;

        private byte d1 = 1, d1_1 = 1;
        private byte d2 = 2, d5 = 5, d7 = 7;
		
        public void test0()
        {
            byte b = 0;
            AssertTrue(b == 0);
        }

        public void test1()
        {
            byte b = 1;
            AssertTrue(b == 1);
        }

        public void test64()
        {
            byte b = 64;
            AssertTrue(b == 64);
        }

        public void test64Instance()
        {
            instanceVar = 64;
            AssertTrue(instanceVar == 64);
        }

        public void test64Static()
        {
            staticVar = 64;
            AssertTrue(staticVar == 64);
        }

        public void test127()
        {
            byte b = 127;
            AssertTrue(b == 127);
        }

        public void test128()
        {
            byte b = 128;
            AssertTrue(b == 128);
        }

        public void test128Instance()
        {
            instanceVar = 128;
            AssertTrue(instanceVar == 128);
        }

        public void test128Static()
        {
            staticVar = 128;
            AssertTrue(staticVar == 128);
        }

        public void test200()
        {
            byte b = 200;
            AssertTrue(b == 200);
        }

        public void test255()
        {
            byte b = 255;
            AssertTrue(b == 255);
        }

        public void testCtr1()
        {
            byte b = new byte();
            AssertTrue(b == 0);
        }

        public void testCtrWithGeneric()
        {
            var creator = new Creator<byte>();
            var b = creator.CreateNew();
            AssertTrue(b == 0);
        }

public void testCompare1()
        {
            AssertTrue(d5 <  d7);
            AssertTrue(d5 <= d7);
            AssertTrue(d7 >  d5);
            AssertTrue(d7 >= d5);
            AssertTrue(d1 >= d1_1);
            AssertTrue(d1 >= d1_1);
            AssertTrue(d1 <= d1_1);
            AssertTrue(d1 == d1_1);

            AssertFalse(d5 >= d7);
            AssertFalse(d5 >  d7);
            AssertFalse(d7 <= d5);
            AssertFalse(d7 <  d5);
            AssertFalse(d1 <  d1_1);
            AssertFalse(d1 >  d1_1);
            AssertFalse(d1 != d1_1);
        }

        public void testTernary()
        {
            AssertEquals(d5 <  d7    ? 5 : 6, 5);
            AssertEquals(d5 <= d7    ? 5 : 6, 5);
            AssertEquals(d7 >  d5    ? 5 : 6, 5);
            AssertEquals(d7 >= d5    ? 5 : 6, 5);
            AssertEquals(d1 >= d1_1  ? 5 : 6, 5);
            AssertEquals(d1 <= d1_1  ? 5 : 6, 5);
            AssertEquals(d1 == d1_1  ? 5 : 6, 5);

            AssertNotSame(d5 >= d7   ? 5 : 6, 5);
            AssertNotSame(d5 >  d7   ? 5 : 6, 5);
            AssertNotSame(d7 <= d5   ? 5 : 6, 5);
            AssertNotSame(d7 <  d5   ? 5 : 6, 5);
            AssertNotSame(d1 <  d1_1 ? 5 : 6, 5);
            AssertNotSame(d1 >  d1_1 ? 5 : 6, 5);
            AssertNotSame(d1 != d1_1 ? 5 : 6, 5);
        }
    }

    public class Creator<T> where T : new()
    {
        public T CreateNew()
        {
            return new T();
        }
    }
}
