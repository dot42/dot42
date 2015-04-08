using Java.Util;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestNewInClass : TestCase
    {
        public class A 
        {
            public static A Create()
            {
                return new A();
            }

            public string Give()
            {
                return "a";
            }
        }

        public class B : A
        {
            public static new B Create()
            {
                return new B();
            }

            public new string Give()
            {
                return "b";
            }
        }

        public class C : B
        {
            public static new C Create()
            {
                return new C();
            }

            public new virtual string Give()
            {
                return "c";
            }
        }

        public class D : C
        {
            public static new D Create()
            {
                return new D();
            }

            public override string Give()
            {
                return "d";
            }
        }

        public class Foo1
        {
            public object foo { get; set; }
        }

        public class Bar1
        {
            public object bar { get; set; }
        }

        public class Foo1<T> : Foo1
        {
            public new T foo { get; set; }

            public T foo2 { get; set; }
        }

        public class FooBar1 : Foo1
        {
            public new Bar1 foo { get; set; }
        }

        public void testInstance1()
        {
            var a = new A();
            AssertEquals("A", "a", a.Give());

            var b = new B();
            AssertEquals("B", "b", b.Give());

            var c = new C();
            AssertEquals("C", "c", c.Give());

            var d = new D();
            AssertEquals("D", "d", d.Give());

            var bAsA = b as A;
            AssertEquals("bAsA", "a", bAsA.Give());

            var cAsA = c as A;
            AssertEquals("cAsA", "a", cAsA.Give());

            var cAsB = c as B;
            AssertEquals("cAsB", "b", cAsB.Give());

            var dAsA = d as A;
            AssertEquals("dAsA", "a", dAsA.Give());

            var dAsB = d as B;
            AssertEquals("dAsB", "b", dAsB.Give());

            var dAsC = d as C;
            AssertEquals("dAsC", "d", dAsC.Give()); // Virtual
        }

        public void testStatic1()
        {
            var a = A.Create();
            AssertEquals("A", "a", a.Give());

            var b = B.Create();
            AssertEquals("B", "b", b.Give());

            var c = C.Create();
            AssertEquals("C", "c", c.Give());

            var d = D.Create();
            AssertEquals("D", "d", d.Give());

            var bAsA = b as A;
            AssertEquals("bAsA", "a", bAsA.Give());
        }

        public void testNewProperty()
        {
            var foo = new Foo1<int>();
            foo.foo = 1;
            ((Foo1) foo).foo = "A";

            AssertEquals(1, foo.foo);
            AssertEquals("A", ((Foo1) foo).foo);
        }

        public void testNewProperty2()
        {
            var foo = new FooBar1();
            foo.foo = new Bar1();
            ((Foo1)foo).foo = "A";

            AssertEquals(typeof(Bar1), foo.foo.GetType());
            AssertEquals("A", ((Foo1)foo).foo);
        }
    }
}
