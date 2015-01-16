using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestDouble : TestCase
    {
        private const double delta = 0.0001;

        public void testSimpleEqual1()
        {
            var i = 5.0;
            AssertTrue(i == 5.0);
        }

        public void testSimpleEqual2()
        {
            var i = 7.0;
            AssertTrue(i == 7.0);
        }

        public void testAdd1()
        {
            var i = 5.0;
            AssertEquals(i + 4.0, 9.0, delta);
        }

        public void testAdd2()
        {
            var i = 5.0;
            AssertTrue(i + 4.0 == 9.0);
        }

        public void testAdd3()
        {
            var i = 5002.0;
            AssertTrue(i + 4.0 == 5006.0);
        }

        public void testAdd4()
        {
            var i = 500002.0;
            AssertTrue(i + 400.0 == 500402.0);
        }

        public void testAdd5()
        {
            var i = 10L;
            var j = 12.0;

            AssertTrue(i + j == 22.0);
        }

        public void testSub1()
        {
            var i = 5.0;
            AssertEquals(i - 4.0, 1.0, delta);
        }

        public void testSub2()
        {
            var i = 5.0;
            AssertTrue(i - 17.0 == -12.0);
        }

        public void testSub3()
        {
            var i = 5002.0;
            AssertTrue(i - 14.0 == 4988.0);
        }

        public void testSub4()
        {
            var i = 500002.0;
            AssertTrue(400.0 - i == -499602.0);
        }

        public void testMul1()
        {
            var i = -2.0;
            AssertEquals(i * 4.0, -8.0, delta);
        }

        public void testMul2()
        {
            var i = 50.0;
            AssertTrue(i * 17.0 == 850.0);
        }

        public void testMul3()
        {
            var i = 5002.0;
            AssertTrue(i * -14.0 == -70028.0);
        }

        public void testDiv1()
        {
            var i = 3.0;
            AssertEquals(i / 2.0, 1.5, delta);
        }

        public void testDiv2()
        {
            var i = 50.0;
            AssertTrue(i / 5.0 == 10.0);
        }

        public void testDiv3()
        {
            var i = 5002.0;
            AssertEquals(i / -14.0,-357.28571428, delta);
        }

        public void testDiv4()
        {
            var i = 0.0;
            AssertTrue(i / 100.0 == 0);
        }

        public void testDiv5()
        {
            var i = 2.0;
            AssertTrue(10.0 / i == 5.0);
        }

        public void testRem1()
        {
            var i = 3.0;
            AssertEquals(i % 2.0, 1.0, delta);
        }

        public void testRem2()
        {
            var i = 50.0;
            AssertTrue(i % 5.0 == 0.0);
        }

        public void testRem3()
        {
            var i = 5002.0;
            AssertTrue(i % -14.0 == 4.0);
        }

        public void testRem4()
        {
            var i = 0.0;
            AssertTrue(i % 100.0 == 0.0);
        }
    }
}
