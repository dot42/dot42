using System;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Dot42.Tests.System.Reflection
{
    [TestFixture]
    public class PropertyInfoTest
    {
        [Test]
        public void Test1()
        {
			// Force include
			var x = new Class1();
			x.MyProperty = 5;
            Assert.AssertEquals(5, x.MyProperty);
			
            var properties = typeof (Class1).GetProperties();
            Assert.AssertEquals(1, properties.Length);
            Assert.AssertEquals("MyProperty", properties[0].Name);
        }

        [Test]
        public void TestGetValue1()
        {
            // Force include
            var x = new Class1();
            x.MyProperty = 5;
            var properties = typeof(Class1).GetProperties();
            Assert.AssertEquals(5, properties[0].GetValue(x));
        }

        [Test]
        public void TestSetValue1()
        {
            // Force include
            var x = new Class1();
            var properties = typeof(Class1).GetProperties();
            properties[0].SetValue(x, 42);
            Assert.AssertEquals(42, x.MyProperty);
        }

        [Test]
        public void TestAttr1()
        {
            // Force include
            var x = new ClassWithAttributes();
            x.MyProperty = 5;
            Assert.AssertEquals(5, x.MyProperty);

            var properties = typeof(ClassWithAttributes).GetProperties();
            Assert.AssertEquals(1, properties.Length);

            var attr = properties[0].GetCustomAttributes(typeof(MyAttribute), false);
            Assert.AssertEquals(1, attr.Length);
        }

        private class Class1
        {
            [Include]
            public int MyProperty { get; set; }
        }

        private class ClassWithAttributes
        {
            [Include]
            [MyAttribute("Hello world")]
            public int MyProperty { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property)]
        private class MyAttribute : Attribute
        {
            public MyAttribute(string message) { }
        }
    }
}

