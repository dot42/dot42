using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Cases
{

    public class TestGenericDelegates : TestCase
    {
        public void testCallGenericStaticMethodCall1()
        {
            Assert.AssertEquals(4, TestGenericStaticMethodCall.Test<int>());
        }

        public void testCallGenericStaticMethodCall2()
        {
            Assert.AssertEquals(4, TestGenericStaticMethodCall.Test1<int, DateTime>());
        }


        public void testInstanceCall()
        {
            Assert.AssertEquals(4, Call(new ClassNongeneric(4).Test));
        }

        public void testStaticCall()
        {
            Assert.AssertEquals(4, Call(ClassNongeneric.TestStatic));
        }

        public void testGenericInstanceCall()
        {
            var instance = new TestGenericInstanceCall<int>(4);
            Assert.AssertEquals(4, Call(instance.Test));
            ;
        }

        public void testGenericExplicitStaticCall()
        {
            Assert.AssertEquals(4, Call(TestGenericStaticCall<int>.Test));
        }

        public void testGenericExplicitStaticMethodCall()
        {
            Assert.AssertEquals(4, Call(TestGenericStaticMethodCall.Test<int>));
        }

        public void testGenericExplicitStaticMethodCall2()
        {
            Assert.AssertEquals(4, Call(TestGenericStaticMethodCall.Test1<int, DateTime>));
        }

        public void testGenericImplicitStaticCall()
        {
            var instance = new TestGenericImplicitStaticCall<int>();
            Assert.AssertEquals(4, Call(instance.Test));
        }

        // this kill the compiler atm
        //public void testGenericMethodOverload1()
        //{
        //    Assert.AssertEquals(4, Call(TestGenericStaticMethodCallSameMethodName.Test<int>));
        //}

        //public void testGenericMethodOverload2()
        //{
        //    Assert.AssertEquals(4, Call(TestGenericStaticMethodCallSameMethodName.Test<int, DateTime>));
        //}


        public int Call(Func<int> del)
        {
            return del();
        }


        internal class ClassNongeneric
        {
            private readonly int _val;

            public ClassNongeneric(int val)
            {
                _val = val;
            }

            public int Test()
            {
                return _val;
            }

            public static int TestStatic()
            {
                return 4;
            }
        }

        internal class TestGenericInstanceCall<T>
        {
            private T val;

            public TestGenericInstanceCall(T val)
            {
                this.val = val;
            }

            public T Test()
            {
                return val;
            }
        }


        internal class TestGenericImplicitStaticCall<T>
        {
            public T Test()
            {
                return (T)(object)4;
            }
        }
        internal class TestGenericStaticCall<T>
        {
            public static T Test()
            {
                return (T)(object)4;
            }

        }

        internal class TestGenericStaticMethodCall
        {
            public static T Test<T>()
            {
                return (T)(object)4;
            }

            public static T Test1<T, T2>()
            {
                if (typeof(T2) != typeof(DateTime))
                    throw new Exception("T2 wrong type.");

                return (T)(object)4;
            }
        }

        //// this will kill the compiler.
        //internal class TestGenericStaticMethodCallSameMethodName
        //{
        //    public static T Test<T>()
        //    {
        //        return (T)(object)4;
        //    }

        //    public static T Test<T, T2>()
        //    {
        //        if (typeof(T2) != typeof(DateTime))
        //            throw new Exception("T2 wrong type.");

        //        return (T)(object)4;
        //    }
        //}

    }
}
