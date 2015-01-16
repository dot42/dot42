using System;
using System.Reflection;
using Junit.Framework;
using Dot42;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestReflection : TestCase
    {
        public void testClassMethods1()
        {
            var objectType = typeof (object);
            var type = typeof (TestClass);
            var methods = type.GetMethods().Length - objectType.GetMethods().Length;
            AssertEquals("#methods", 2, methods);
        }

        public void testClassMethods2()
        {
            var objectType = typeof(object);
            var type = typeof(TestClass);
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Length;
            AssertEquals("#methods", 2, methods);
        }

        public class TestClass
        {
			[Include]
            public int Foo()
            {
                return 0;
            }
			[Include]
            public int Dot()
            {
                return 0;
            }
        }
    }
}
