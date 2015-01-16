using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestShort : TestCase
    {
        public void testSimpleEqual1()
        {
            short i = 5;
            AssertTrue(i == 5);
        }

        public void testSimpleEqual2()
        {
            short i = 7;
            AssertTrue(i == 7);
        }

        public void testAdd1()
        {
            short i = 5;
            AssertEquals(i + 4, 9);
        }

        public void testAdd2()
        {
            short i = 5;
            AssertTrue(i + 4 == 9);
        }

        public void testAdd3()
        {
            short i = 5002;
            AssertTrue(i + 4 == 5006);
        }

        public void testAdd4()
        {
            short i = 30002;
            AssertTrue(i + 400 == 30402);
        }

        public void testSub1()
        {
            short i = 5;
            AssertEquals(i - 4, 1);
        }

        public void testSub2()
        {
            short i = 5;
            AssertTrue(i - 17 == -12);
        }

        public void testSub3()
        {
            short i = 5002;
            AssertTrue(i - 14 == 4988);
        }

        public void testSub4()
        {
            short i = 20002;
            AssertTrue(400 - i == -19602);
        }

        public void testMul1()
        {
            short i = -2;
            AssertEquals(i * 4, -8);
        }

        public void testMul2()
        {
            short i = 50;
            AssertTrue(i * 17 == 850);
        }

        public void testMul3()
        {
            short i = 5002;
            AssertTrue(i * -14 == -70028);
        }

        public void testMul4()
        {
            short i = 2;
            AssertTrue(10 / i == 5);
        }

        public void testDiv1()
        {
            short i = 3;
            AssertEquals(i  / 2, 1);
        }

        public void testDiv2()
        {
            short i = 50;
            AssertTrue(i / 5 == 10);
        }

        public void testDiv3()
        {
            short i = 5002;
            AssertTrue(i / -14 == -357);
        }

        public void testDiv4()
        {
            short i = 0;
            AssertTrue(i /100 == 0);
        }

        public void testRem1()
        {
            short i = 3;
            AssertEquals(i % 2, 1);
        }

        public void testRem2()
        {
            short i = 50;
            AssertTrue(i % 5 == 0);
        }

        public void testRem3()
        {
            short i = 5002;
            AssertTrue(i % -14 == 4);
        }

        public void testRem4()
        {
            short i = 0;
            AssertTrue(i % 100 == 0);
        }

        public void testCtor1()
        {
            var s = new short((short)42);

            AssertEquals((short)42, s);
        }

        public void testCtor2()
        {
            var i = 42;
            var s = new short((short)i);

            AssertEquals((short)42, s);
        }

        public void testCtor3()
        {
            var s = new short(42);

            AssertEquals((short)42, s);
        }

        public void testCtor4()
        {
            var b = (byte) 42;
            var s = new short((short)b);

            AssertEquals((short)42, s);
        }
    }
}
