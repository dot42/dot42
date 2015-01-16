using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestUShort : TestCase
    {
        [Include]
        public const UInt16 SIGN_MASK = ((UInt16)1 << 15);


        static readonly ushort[] STATIC_USHORT2 = { 0xffff, 0xffff, 0xffff, 0 };
        
        public void testSimpleEqual1()
        {
            ushort i = 5;
            AssertTrue(i == 5);
        }

        public void testSimpleEqual2()
        {
            ushort i = 7;
            AssertTrue(i == 7);
        }

        public void testAdd1()
        {
            ushort i = 5;
            AssertEquals(i + 4, 9);
        }

        public void testAdd2()
        {
            ushort i = 5;
            AssertTrue(i + 4 == 9);
        }

        public void testAdd3()
        {
            ushort i = 5002;
            AssertTrue(i + 4 == 5006);
        }

        public void testAdd4()
        {
            ushort i = 30002;
            AssertTrue(i + 400 == 30402);
        }

        public void testSub1()
        {
            ushort i = 5;
            AssertEquals(i - 4, 1);
        }

        public void testSub2()
        {
            ushort i = 5;
            AssertTrue(i - 17 == -12);
        }

        public void testSub3()
        {
            ushort i = 5002;
            AssertTrue(i - 14 == 4988);
        }

        public void testSub4()
        {
            ushort i = 20002;
            AssertTrue(400 - i == -19602);
        }

        public void testMul1()
        {
            ushort i = 2;
            AssertEquals(i * 4, 8);
        }

        public void testMul2()
        {
            ushort i = 50;
            AssertTrue(i * 17 == 850);
        }

        public void testMul3()
        {
            ushort i = 5002;
            AssertTrue(i * -14 == -70028);
        }

        public void testMul4()
        {
            ushort i = 2;
            AssertTrue(10 / i == 5);
        }

        public void testDiv1()
        {
            ushort i = 3;
            AssertEquals(i  / 2, 1);
        }

        public void testDiv2()
        {
            ushort i = 50;
            AssertTrue(i / 5 == 10);
        }

        public void testDiv3()
        {
            ushort i = 5002;
            AssertTrue(i / 14 == 357);
        }

        public void testDiv4()
        {
            ushort i = 0;
            AssertTrue(i /100 == 0);
        }

        public void testRem1()
        {
            ushort i = 3;
            AssertEquals(i % 2, 1);
        }

        public void testRem2()
        {
            ushort i = 50;
            AssertTrue(i % 5 == 0);
        }

        public void testRem3()
        {
            ushort i = 5002;
            AssertTrue(i % 14 == 4);
        }

        public void testRem4()
        {
            ushort i = 0;
            AssertTrue(i % 100 == 0);
        }

        public void testStatic()
        {
            AssertNotNull(STATIC_USHORT2);
            AssertSame(4, STATIC_USHORT2.Length);
            AssertSame((ushort)0xffff, STATIC_USHORT2[0]);
            AssertSame((ushort)0xffff, STATIC_USHORT2[1]);
            AssertSame((ushort)0xffff, STATIC_USHORT2[2]);
            AssertSame((ushort)0, STATIC_USHORT2[3]);
        }
    }
}
