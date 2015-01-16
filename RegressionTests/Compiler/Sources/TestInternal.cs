using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestInternal : TestCase
    {
        public void testInternalStaticMethod()
        {
            var im1 = new IM1();
            var result = im1.Foo1();

            AssertEquals(42, result);
        }

        public void testInternalConst()
        {
            var im1 = new IM1();
            var result = im1.Foo2();

            AssertEquals(62135596800000L, result);
        }

        public void testInternalStruct()
        {
            var myInternalStruct = new MyInternalStruct(new MyInternalClass());

            AssertEquals(17, myInternalStruct.C.X);
        }

        public class IM1
        {
            public int Foo1()
            {
                return IM2.Foo4();
            }

            public long Foo2()
            {
                return IM2.Foo3;
            }
        }

        public class IM2
        {
            internal const long Foo3 =  62135596800000L;

            internal static int Foo4()
            {
                return 42;
            }
        }

        internal class MyInternalClass
        {
            public int X = 17;
        }

        internal struct MyInternalStruct
        {
            public MyInternalClass C;

            public MyInternalStruct(MyInternalClass c)
            {
                C = c;
            }
        }
    }

   
}
