using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestInstanceOf : TestCase
    {
        public void test1()
        {
            var obj = "hello";
            AssertTrue("obj is string", IsString(obj));
        }

        public void test2()
        {
            var obj = new TestInstanceOf();
            AssertFalse("obj is not string", IsString(obj));
        }

        public bool IsString(object x)
        {
            return x is string;
        }

        public void test4()
        {
            var obj = new Derived1();
            AssertEquals("Derived1", 1, Foo(obj));
        }

        public void test5a()
        {
            IBase obj = new Derived2();
            AssertEquals("Derived2", 22, Foo(obj));
        }

        public void test5b()
        {
            IBase obj = new Derived2();
            AssertEquals("Derived2", 12, Foo((Derived2)obj));
        }

        public int Foo(IBase x)
        {
            if (x is Derived2)
            {
                return ((Derived2)x).FooX();
            }
            return x.Foo();
        }

        public int Foo(Derived2 x)
        {
            return x.Foo() + 10;
        }



        public interface IBase
        {
            int Foo();
        }

        public class Derived1 : IBase
        {
            public int Foo()
            {
                return 1;
            }
        }

        public class Derived2 : IBase
        {
            public int Foo()
            {
                return 2;
            }

            public int FooX()
            {
                return 22;
            }
        }
    }
}
