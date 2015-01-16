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
    }
}
