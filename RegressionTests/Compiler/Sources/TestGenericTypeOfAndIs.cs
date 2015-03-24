using System;
using Junit.Framework;
using Android.Graphics;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericTypeOfAndIs : TestCase
    {
        class GenericClass<T>
        {
            private readonly T _val;

            public GenericClass(T val)
            {
                _val = val;
            }

            public Type GetTypeofT()
            {
                return typeof (T);
            }

            public Type GetTypeofVal()
            {
                return _val.GetType();
            }

            public bool IsT(object o)
            {
                return o is T;
            }

            public bool ValIsT()
            {
                return ((object)_val) is T;
            }

            public bool ValIsInt()
            {
                return ((object)_val) is int;
            }

            public bool ValIsDateTime()
            {
                return ((object)_val) is DateTime;
            }

        }

        public void test1()
        {
            AssertTrue(new GenericClass<int>(42).ValIsT());
            AssertFalse(new GenericClass<int>(42).ValIsDateTime());
            AssertFalse(new GenericClass<int>(42).IsT(null));
            AssertTrue(new GenericClass<int>(42).ValIsInt());
        }

        public void test2()
        {
            AssertFalse(new GenericClass<Derived1>(null).IsT(new object()));
            AssertFalse(new GenericClass<Derived1>(null).IsT(new Base()));
            AssertTrue(new GenericClass<Derived1>(null).IsT(new Derived1()));
            AssertSame(new GenericClass<Derived1>(null).GetTypeofT(), typeof(Derived1));
            AssertSame(new GenericClass<Derived1>(new Derived1()).GetTypeofVal(), typeof(Derived1));
            AssertNotSame(new GenericClass<Derived1>(null).GetTypeofT(), typeof(object));
        }

        public void test3()
        {
            AssertFalse(new GenericClass<IDerived>(null).IsT(new object()));
            AssertFalse(new GenericClass<IDerived>(null).IsT(new Base()));
            AssertTrue(new GenericClass<IDerived>(null).IsT(new Derived1()));
        }

        public void test4()
        {
            AssertFalse(new GenericClass<DateTime>(DateTime.Now).IsT(new object()));
            AssertFalse(new GenericClass<DateTime>(DateTime.Now).IsT(new Base()));
            AssertSame(new GenericClass<DateTime>(DateTime.Now).GetTypeofT(), typeof(DateTime));
            AssertNotSame(new GenericClass<DateTime>(DateTime.Now).GetTypeofT(), typeof(object));
        }

        public void testPrivitiveTypeOfDoesntMatchT_KnownToFail()
        {
            AssertSame(new GenericClass<int>(42).GetTypeofT(), typeof(int));
        }

        public void testPrivitiveTypeOfDoesntMatchT2_KnownToFail()
        {
            AssertSame(new GenericClass<int>(42).GetTypeofVal(), typeof(int));
        }
    
        public class Base
        {
            public virtual int Foo()
            {
                return 0;
            }
        }

        public interface IDerived
        {
            int Foo();
        }

        public class Derived1 : Base, IDerived
        {
            public override int Foo()
            {
                return 1;
            }
        }
    }
}
