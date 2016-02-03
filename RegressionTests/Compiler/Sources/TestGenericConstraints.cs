using System;
using System.Collections;
using System.Collections.Generic;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericConstraints : TestCase
    {
        public void testMethod1()
        {
            var x = new MyClass();
            AssertEquals(5, MyClassFoo(x));
        }

        public void testMethod2()
        {
            var x = new MyClass2();
            AssertEquals(15, MyClassFoo2(x));
        }

        public void testGClass1()
        {
            var x = new GClass<MyClass>(new MyClass());
            AssertEquals(5, x.GetValue());
        }

        public void testGClass2()
        {
            var x = new GClass2<MyClass3>(new MyClass3());
            AssertEquals(15, x.GetValue());
        }

        public void testGClass4()
        {
            var x = new GenericWithTypeContraint<List<string>>();
            AssertEquals(0, x.x);
        }

        private class MyClass
        {
            public int MyClassFoo()
            {
                return 5;
            }
        }

        private interface IMyIntf
        {
            int MyIntfFoo();
        }

        private class MyClass2 : IMyIntf
        {
            public int MyClassFoo2()
            {
                return 10;
            }

            public int MyIntfFoo()
            {
                return 15;
            }
        }

        private class MyClass3 : MyClass2, IDisposable
        {
            public void Dispose()
            {
            }
        }

        private static int MyClassFoo<T>(T instance)
            where T:  MyClass
        {
            return instance.MyClassFoo();
        }

        private static int MyClassFoo2<T>(T instance)
            where T : MyClass2, IMyIntf
        {
            return instance.MyIntfFoo();
        }

        private class GClass<T>
            where T : MyClass
        {
            private readonly T x;

            public GClass(T x)
            {
                this.x = x;
            }

            public int GetValue()
            {
                return x.MyClassFoo();
            }
        }

        private class GClass2<T>
            where T : MyClass2, IMyIntf, IDisposable
        {
            private readonly T x;

            public GClass2(T x)
            {
                this.x = x;
            }

            public int GetValue()
            {
                return x.MyIntfFoo();
            }
        }

        class NonGenericBaseClass
        {
            public int x;

            public int GetX()
            {
                return x;
            }
            protected int Test(List<string> xx)
            {
                xx.Add("test");
                return 0;
            }
        }

        class GenericWithTypeContraint<T> : NonGenericBaseClass where T : List<string>, IList, new()
        {
            private T x;
            public GenericWithTypeContraint()
            {
                x = new T();
                Test(x);
            }
        }
    }
}
