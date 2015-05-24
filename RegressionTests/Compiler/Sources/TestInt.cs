using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestInt : TestCase
    {
        private int d1 = 1, d1_1 = 1;
        private int d2 = 2, d5 = 5, d7 = 7;

        public void testSimpleEqual1()
        {
            var i = 5;
            AssertTrue(i == 5);
        }

        public void testSimpleEqual2()
        {
            var i = 7;
            AssertTrue(i == 7);
        }

        public void testAdd1()
        {
            var i = 5;
            AssertEquals(i + 4, 9);
        }

        public void testAdd2()
        {
            var i = 5;
            AssertTrue(i + 4 == 9);
        }

        public void testAdd3()
        {
            var i = 5002;
            AssertTrue(i + 4 == 5006);
        }

        public void testAdd4()
        {
            var i = 500002;
            AssertTrue(i + 400 == 500402);
        }

        public void testSub1()
        {
            var i = 5;
            AssertEquals(i - 4, 1);
        }

        public void testSub2()
        {
            var i = 5;
            AssertTrue(i - 17 == -12);
        }

        public void testSub3()
        {
            var i = 5002;
            AssertTrue(i - 14 == 4988);
        }

        public void testSub4()
        {
            var i = 500002;
            AssertTrue(400 - i == -499602);
        }

        public void testMul1()
        {
            var i = -2;
            AssertEquals(i * 4, -8);
        }

        public void testMul2()
        {
            var i = 50;
            AssertTrue(i * 17 == 850);
        }

        public void testMul3()
        {
            var i = 5002;
            AssertTrue(i * -14 == -70028);
        }

        public void testMul4()
        {
            var i = 2;
            AssertTrue(10 / i == 5);
        }

        public void testDiv1()
        {
            var i = 3;
            AssertEquals(i  / 2, 1);
        }

        public void testDiv2()
        {
            var i = 50;
            AssertTrue(i / 5 == 10);
        }

        public void testDiv3()
        {
            var i = 5002;
            AssertTrue(i / -14 == -357);
        }

        public void testDiv4()
        {
            var i = 0;
            AssertTrue(i /100 == 0);
        }

        public void testRem1()
        {
            var i = 3;
            AssertEquals(i % 2, 1);
        }

        public void testRem2()
        {
            var i = 50;
            AssertTrue(i % 5 == 0);
        }

        public void testRem3()
        {
            var i = 5002;
            AssertTrue(i % -14 == 4);
        }

        public void testRem4()
        {
            var i = 0;
            AssertTrue(i % 100 == 0);
        }

        public void testCompare1()
        {
            AssertTrue(d5 < d7);
            AssertTrue(d5 <= d7);
            AssertTrue(d7 > d5);
            AssertTrue(d7 >= d5);
            AssertTrue(d1 >= d1_1);
            AssertTrue(d1 >= d1_1);
            AssertTrue(d1 <= d1_1);
            AssertTrue(d1 == d1_1);

            AssertFalse(d5 >= d7);
            AssertFalse(d5 > d7);
            AssertFalse(d7 <= d5);
            AssertFalse(d7 < d5);
            AssertFalse(d1 < d1_1);
            AssertFalse(d1 > d1_1);
            AssertFalse(d1 != d1_1);
        }

        public void testTernary()
        {
            AssertEquals(d5 < d7 ? 5 : 6, 5);
            AssertEquals(d5 <= d7 ? 5 : 6, 5);
            AssertEquals(d7 > d5 ? 5 : 6, 5);
            AssertEquals(d7 >= d5 ? 5 : 6, 5);
            AssertEquals(d1 >= d1_1 ? 5 : 6, 5);
            AssertEquals(d1 <= d1_1 ? 5 : 6, 5);
            AssertEquals(d1 == d1_1 ? 5 : 6, 5);

            AssertNotSame(d5 >= d7 ? 5 : 6, 5);
            AssertNotSame(d5 > d7 ? 5 : 6, 5);
            AssertNotSame(d7 <= d5 ? 5 : 6, 5);
            AssertNotSame(d7 < d5 ? 5 : 6, 5);
            AssertNotSame(d1 < d1_1 ? 5 : 6, 5);
            AssertNotSame(d1 > d1_1 ? 5 : 6, 5);
            AssertNotSame(d1 != d1_1 ? 5 : 6, 5);
        }
    }
}
