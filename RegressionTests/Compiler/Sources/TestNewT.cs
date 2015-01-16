using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestNewT : TestCase
    {
        public void testNewObjT1()
        {
            object x = NewObj<MyClass>();
            AssertTrue(x is MyClass);
        }

        public void testGClass1()
        {
            object x = new GClass<MyClass>().NewObj();
            AssertTrue(x is MyClass);
        }

        public void testGClass2a()
        {
            object x = new GClass2<MyClass, MyClass2>().NewT1();
            AssertTrue(x is MyClass);
        }

        public void testGClass2b()
        {
            object x = new GClass2<MyClass, MyClass2>().NewT2();
            AssertTrue(x is MyClass2);
        }

        public void testGClass3a()
        {
            object x = new GClass3<MyClass, MyClass2>().NewT1();
            AssertTrue(x is MyClass);
        }

        public void testGClass3b()
        {
            object x = new GClass3<MyClass, MyClass2>().NewT2();
            AssertTrue(x is MyClass2);
        }

        public void testGClass3c()
        {
            object x = new GClass3<MyClass, MyClass2>().NewObj();
            AssertTrue(x is MyClass2);
        }

        public static T NewObj<T>()
            where T : class, new()
        {
            return new T();
        }

        public class MyClass
        {            
        }

        public class MyClass2
        {
        }

        public class GClass<T>
            where T : class, new()
        {
            public object NewObj()
            {
                return new T();
            }
        }

        public class GClass2<T1, T2>
            where T1 : class, new()
            where T2 : class, new()
        {
            public object NewT1()
            {
                return new T1();
            }
            public object NewT2()
            {
                return new T2();
            }
        }

        public class BaseClass<T>
            where T : class, new()
        {
            public object NewObj()
            {
                return new T();
            }
        }

        public class GClass3<T1, T2> : BaseClass<T2> 
            where T1 : class, new()
            where T2 : class, new()
        {
            public object NewT1()
            {
                return new T1();
            }
            public object NewT2()
            {
                return new T2();
            }
        }
    }
}
