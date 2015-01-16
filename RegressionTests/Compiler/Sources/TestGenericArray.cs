using System;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericArray : TestCase
    {
        public void test1()
        {
            var arr = CreateArray<string>();
            var type = arr.GetType();
            AssertTrue(type.IsArray);
            AssertEquals("String", type.GetElementType().Name);
        }

        public void testGClass1()
        {
            var arr = new GClass<string>().CreateArray();
            var type = arr.GetType();
            AssertTrue(type.IsArray);
            AssertEquals("String", type.GetElementType().Name);
        }

        public void testGClass2()
        {
            var arr = new GClass<string>().CreateArrayT<Java.Lang.Package>();
            var type = arr.GetType();
            AssertTrue(type.IsArray);
            AssertEquals("String", type.GetElementType().Name);
        }

        public void testGClass3()
        {
            var arr = new GClass<string>().CreateArrayY<Java.Lang.Package>();
            var type = arr.GetType();
            AssertTrue(type.IsArray);
            AssertEquals("Package", type.GetElementType().Name);
        }

        private static object CreateArray<T>()
        {
            return new T[1];
        }

        private class GClass<T>
        {
            public object CreateArray()
            {
                return new T[1];
            }
            public object CreateArrayT<Y>()
            {
                return new T[1];
            }
            public object CreateArrayY<Y>()
            {
                return new Y[1];
            }
        }
    }
}
