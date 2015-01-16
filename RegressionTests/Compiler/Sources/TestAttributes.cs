using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestAttributes : TestCase
    {
        public void testClassAttributes1()
        {
            var type = typeof (TestClass);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 2, attr.Length);
        }

        public void testClassAttributes2()
        {
            var type = typeof(TestClass2);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 1, attr.Length);

            AssertEquals("#x", 5, ((MyAttribute)attr[0]).GetX());
            AssertEquals("#y", 0, ((MyAttribute)attr[0]).GetY());
        }

        public void testClassAttributes3()
        {
            var type = typeof(TestClass3);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes should be 1", 1, attr.Length);

            AssertEquals("#x", 7, ((MyAttribute)attr[0]).GetX());
            AssertEquals("#y", 7, ((MyAttribute)attr[0]).GetY());
        }

        public void testClassAttributes4()
        {
            var type = typeof(TestClass);
            var attr = type.GetCustomAttributes(false);
            AssertEquals("#attributes", 2, attr.Length);

            var first = (MyAttribute) attr[0];
            var second = (MyAttribute) attr[1];
            if (first.GetX() == 5) AssertEquals("#y0", 0, first.GetY());
            else AssertEquals("#y0", 7, first.GetY());
            if (second.GetX() == 7) AssertEquals("#y0", 7, second.GetY());
            else AssertEquals("#y0", 0, second.GetY());
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        public class MyAttribute : Attribute
        {
            private readonly int x;
            private readonly int y;

            public MyAttribute(int x)
            {
                this.x = x;
            }

            public MyAttribute(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int GetX()
            {
                return x;
            }

            public int GetY()
            {
                return y;
            }
        }

        [MyAttribute(5)]
        [MyAttribute(7, 7)]
        public class TestClass { }

        [MyAttribute(5)]
        public class TestClass2 { }

        [MyAttribute(7, 7)]
        public class TestClass3 { }
    }
}
