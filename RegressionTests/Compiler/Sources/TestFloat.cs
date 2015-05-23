using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestFloat : TestCase
    {
        private const float delta = 0.0001f;

        private float d1 = 1, d1_1 = 1;
        private float d2 = 2, d5 = 5, d7 = 7;
        private float dNaN = float.NaN;

        public void testSimpleEqual1()
        {
            var i = 5.0f;
            AssertTrue(i == 5.0f);
        }

        public void testSimpleEqual2()
        {
            var i = 7.0f;
            AssertTrue(i == 7.0f);
        }

        public void testAdd1()
        {
            var i = 5.0f;
            AssertEquals(i + 4.0f, 9.0f, delta);
        }

        public void testAdd2()
        {
            var i = 5.0f;
            AssertTrue(i + 4.0f == 9.0f);
        }

        public void testAdd3()
        {
            var i = 5002.0f;
            AssertTrue(i + 4.0f == 5006.0f);
        }

        public void testAdd4()
        {
            var i = 500002.0f;
            AssertTrue(i + 400.0f == 500402.0f);
        }

        public void testSub1()
        {
            var i = 5.0f;
            AssertEquals(i - 4.0f, 1.0f, delta);
        }

        public void testSub2()
        {
            var i = 5.0f;
            AssertTrue(i - 17.0f == -12.0f);
        }

        public void testSub3()
        {
            var i = 5002.0f;
            AssertTrue(i - 14.0f == 4988.0f);
        }

        public void testSub4()
        {
            var i = 500002.0f;
            AssertTrue(400.0f - i == -499602.0f);
        }

        public void testMul1()
        {
            var i = -2.0f;
            AssertEquals(i * 4.0f, -8.0f, delta);
        }

        public void testMul2()
        {
            var i = 50.0f;
            AssertTrue(i * 17.0f == 850.0f);
        }

        public void testMul3()
        {
            var i = 5002.0f;
            AssertTrue(i * -14.0f == -70028.0f);
        }

        public void testMul4()
        {
            var i = 2.0f;
            AssertTrue(10.0f / i == 5.0f);
        }

        public void testDiv1()
        {
            var i = 3.0f;
            AssertEquals(i / 2.0f, 1.5f, delta);
        }

        public void testDiv2()
        {
            var i = 50.0f;
            AssertTrue(i / 5.0f == 10.0f);
        }

        public void testDiv3()
        {
            var i = 5002.0f;
            AssertEquals(i / -14.0f, -357.28571428f, delta);
        }

        public void testDiv4()
        {
            var i = 0.0f;
            AssertTrue(i / 100.0f == 0f);
        }

        public void testRem1()
        {
            var i = 3.0f;
            AssertEquals(i % 2.0f, 1.0f, delta);
        }

        public void testRem2()
        {
            var i = 50.0f;
            AssertTrue(i % 5.0f == 0.0f);
        }

        public void testRem3()
        {
            var i = 5002.0f;
            AssertTrue(i % -14.0f == 4.0f);
        }

        public void testRem4()
        {
            var i = 0.0f;
            AssertTrue(i % 100.0f == 0.0f);
        }

        public void testCompare1()
        {
            AssertTrue(d5 < d7);
            AssertTrue(d5 <= d7);
            AssertTrue(d7 > d5);
            AssertTrue(d7 >= d5);
            AssertTrue(d1 >= d1_1);
            AssertTrue(d1 <= d1_1);
        }

        public void testCompare2()
        {
            AssertFalse(d5 >= d7);
            AssertFalse(d5 > d7);
            AssertFalse(d7 <= d5);
            AssertFalse(d7 < d5);
            AssertFalse(d1 < d1_1);
            AssertFalse(d1 > d1_1);
        }

        public void testCompareNaN()
        {
            AssertFalse(dNaN == double.NaN);
        }

        public void testCompareNaN1()
        {
            AssertFalse(d5   <  dNaN);
            AssertFalse(dNaN <  d5);

            AssertFalse(d5   <= dNaN);
            AssertFalse(dNaN <= d5);

            AssertFalse(dNaN <  double.NaN);
            AssertFalse(dNaN <= double.NaN);
        }

        public void testCompareNaN2()
        {
            AssertFalse(d5   >  dNaN);
            AssertFalse(dNaN >  d5);

            AssertFalse(d5   >= dNaN);
            AssertFalse(dNaN >= d5);

            AssertFalse(dNaN >  double.NaN);
            AssertFalse(dNaN >= double.NaN);
        }
    }
}
