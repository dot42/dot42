using Java.Util;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestNewInClassWithInterface : TestCase
    {
		public interface IGive 
		{
            string Give();
		}
		
        public class A : IGive
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

        public class B : A, IGive
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

        public class C : B, IGive
        {
            public static new C Create()
            {
                return new C();
            }

            public new string Give()
            {
                return "c";
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

            var bAsA = b as A;
            AssertEquals("bAsA", "a", bAsA.Give());

            var cAsA = c as A;
            AssertEquals("cAsA", "a", cAsA.Give());

            var cAsB = c as B;
            AssertEquals("cAsB", "b", cAsB.Give());
        }

        public void testInterface1()
        {
            IGive a = new A();
            AssertEquals("A", "a", a.Give());

            IGive b = new B();
            AssertEquals("B", "b", b.Give());

            IGive c = new C();
            AssertEquals("C", "c", c.Give());
        }

        public void testStatic1()
        {
            var a = A.Create();
            AssertEquals("A", "a", a.Give());

            var b = B.Create();
            AssertEquals("B", "b", b.Give());

            var c = C.Create();
            AssertEquals("C", "c", c.Give());

            var bAsA = b as A;
            AssertEquals("bAsA", "a", bAsA.Give());
        }
    }
}
