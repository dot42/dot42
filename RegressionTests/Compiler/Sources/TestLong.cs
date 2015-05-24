using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestLong : TestCase
    {
        private long d1 = 1, d1_1 = 1;
        private long d2 = 2, d5 = 5, d7 = 7;

        public void testSimpleEqual1()
        {
            var i = 5L;
            AssertTrue(i == 5L);
        }

        public void testSimpleEqual2()
        {
            var i = 7L;
            AssertTrue(i == 7L);
        }

        public void testAdd1()
        {
            var i = 5L;
            AssertEquals(i + 4L, 9L);
        }

        public void testAdd2()
        {
            var i = 5L;
            AssertTrue(i + 4L == 9L);
        }

        public void testAdd3()
        {
            var i = 5002L;
            AssertTrue(i + 4L == 5006L);
        }

        public void testAdd4()
        {
            var i = 500002L;
            AssertTrue(i + 400L == 500402L);
        }

        public void testSub1()
        {
            var i = 5L;
            AssertEquals(i - 4L, 1L);
        }

        public void testSub2()
        {
            var i = 5L;
            AssertTrue(i - 17L == -12L);
        }

        public void testSub3()
        {
            var i = 5002L;
            AssertTrue(i - 14L == 4988L);
        }

        public void testSub4()
        {
            var i = 500002L;
            AssertTrue(400L - i == -499602L);
        }

        public void testMul1()
        {
            var i = -2L;
            AssertEquals(i * 4L, -8L);
        }

        public void testMul2()
        {
            var i = 50L;
            AssertTrue(i * 17L == 850L);
        }

        public void testMul3()
        {
            var i = 5002L;
            AssertTrue(i * -14L == -70028L);
        }

        public void testMul4()
        {
            var i = 2;
            AssertTrue(10L * i == 20L);
        }

        public void testDiv1()
        {
            var i = 3L;
            AssertEquals(i  / 2L, 1L);
        }

        public void testDiv2()
        {
            var i = 50L;
            AssertTrue(i / 5L == 10L);
        }

        public void testDiv3()
        {
            var i = 5002L;
            AssertTrue(i / -14L == -357L);
        }

        public void testDiv4()
        {
            var i = 0L;
            AssertTrue(i /100L == 0L);
        }

        public void testDiv5()
        {
            var i = 2L;
            AssertTrue(10L / i == 5L);
        }

        public void testRem1()
        {
            var i = 3L;
            AssertEquals(i % 2L, 1L);
        }

        public void testRem2()
        {
            var i = 50L;
            AssertTrue(i % 5L == 0L);
        }

        public void testRem3()
        {
            var i = 5002L;
            AssertTrue(i % -14L == 4L);
        }

        public void testRem4()
        {
            var i = 0L;
            AssertTrue(i % 100L == 0L);
        }

        public void testGen1()
        {
            var i = 2L;
            var j = 3L;
            var k = 1;
            AssertTrue(i+j*k == 5L);
        }

        public void testLarge()
        {
            var i = 0xFFFFFFFFL;
            AssertTrue(i>0);
        }

        public void testClass1()
        {
            var propMethod = Class1Method.GetPropMethod();
            AssertEquals(42, propMethod);
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

    internal class Class1Prop
    {
        public static Class1Prop Instance
        {
            get
            {
                {
                    return new Class1Prop();
                }
            }
        }

        public long Prop
        {
            get { return 42; }
        }
    }

    internal class Class1Method
    {
        public static long GetPropMethod()
        {
            return Class1Prop.Instance.Prop;
        }
    }
}
