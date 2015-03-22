using System;
using Junit.Framework;

namespace dot42.CompilerBugTesting
{
    public class TestDelegateToGeneric : TestCase
    {
        public void testNongenericInstanceCall()
        {
            AssertEquals(4, new NongenericClass(4).Test());
            AssertEquals(4, Call(new NongenericClass(4).Test));
        }

        public void testNongenericStaticCall()
        {
            AssertEquals(4, NongenericClass.TestStatic());
            AssertEquals(4, Call(NongenericClass.TestStatic));
        }

        public void testGenericInstanceCall()
        {
            var instance = new GenericClass<int>(4);
            AssertEquals(4, instance.Test());
            AssertEquals(4, Call(instance.Test));
            ;
        }

        public void testGenericStaticCall()
        {
            Assert.AssertEquals(4, GenericClassStaticMethod<int>.Test());
            Assert.AssertEquals(4, Call(GenericClassStaticMethod<int>.Test));
        }


        public void testGenericInstanceCallToGenericMethod()
        {
            var instance = new GenericClassGenericMethod<int>(4);
            AssertEquals(4, instance.Test<DateTime>());
            AssertEquals(4, Call(instance.Test<DateTime>));
        }

        public void testNongenericClassGenericMethodInstanceCall()
        {
            var instance = new NongenericClassGenericMethod(4);
            Assert.AssertEquals(4, instance.Test<int>());
            Assert.AssertEquals(4, Call(instance.Test<int>));
        }

        public void testNongenericClassGenericMethodStaticCall()
        {
            Assert.AssertEquals(4, NongenericClassGenericStaticMethod.Test<int>());
            Assert.AssertEquals(4, Call(NongenericClassGenericStaticMethod.Test<int>));
        }

        public void testNongenericClassGenericMethodStaticCall2()
        {
            Assert.AssertEquals(4, NongenericClassGenericStaticMethod.Test1<int, DateTime>());
            Assert.AssertEquals(4, Call(NongenericClassGenericStaticMethod.Test1<int, DateTime>));
        }

        public void testGenericClassGenericMethodStaticCall()
        {
            Assert.AssertEquals(4, GenericClassGenericStaticMethod<int>.Test<DateTime>());
            Assert.AssertEquals(4, Call(GenericClassGenericStaticMethod<int>.Test<DateTime>));
        }

        // this will kill the compiler atm.
        //public void testNongenericClassGenericStaticMethodGenericOverload()
        //{
        //    var instance = new NongenericClassGenericStaticMethodGenericOverload(4);
        //    Assert.AssertEquals(4, Call(instance.Test<long>));
        //}


        private int Call(Func<int> del)
        {
            return del();
        }


        internal class NongenericClass
        {
            private readonly int _val;

            public NongenericClass(int val)
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

        internal class GenericClass<T>
        {
            private T val;

            public GenericClass(T val)
            {
                this.val = val;
            }

            public T Test()
            {
                Assert.AssertTrue(val is int);
                //Assert.AssertEquals(typeof(int), typeof(T));
                return val;
            }
        }


        internal class NongenericClassGenericMethod
        {
            private readonly int _val;

            public NongenericClassGenericMethod(int val)
            {
                _val = val;
            }

            public T Test<T>()
            {
                Assert.AssertTrue(_val is int);
                //Assert.AssertTrue(typeof(int).IsAssignableFrom(typeof(T)));
                //Assert.AssertEquals(typeof(int), typeof(T));
                //Assert.AssertEquals(typeof(int), typeof(T));

                return (T)(object)_val;
            }
        }

        internal class GenericClassStaticMethod<T>
        {
            public static T Test()
            {
                //Assert.AssertTrue(typeof(int).IsAssignableFrom(typeof(T)));
                //Assert.AssertEquals(typeof(int), typeof(T));

                return (T)(object)4;
            }

        }

        internal class GenericClassGenericStaticMethod<T>
        {
            public static T Test<T2>()
            {
                //Assert.AssertEquals(typeof(int), typeof(T));
                Assert.AssertEquals(typeof(DateTime), typeof(T2));
                return (T)(object)4;
            }

        }

        internal class GenericClassGenericMethod<T>
        {
            private readonly T _value;
            public GenericClassGenericMethod(T value)
            {
                _value = value;
            }
            public T Test<T2>()
            {
                //Assert.AssertEquals(typeof(int), typeof(T));
                Assert.AssertEquals(typeof(DateTime), typeof(T2));
                return _value;
            }

        }

        internal class NongenericClassGenericStaticMethod
        {
            public static T Test<T>()
            {
                return (T)(object)4;
            }

            public static T Test1<T, T2>()
            {
                //Assert.AssertEquals(typeof(int), typeof(T));

                Assert.AssertEquals(typeof(DateTime), typeof(T2));

                return (T)(object)4;
            }
        }

        // this will kill the compiler.
        //internal class NongenericClassGenericStaticMethodGenericOverload
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
