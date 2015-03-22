using System;
using Java.Lang;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestTypeOf : TestCase
    {
        public void test1()
        {
            var type = typeof (TestTypeOf);
            AssertEquals("TestTypeOf", type.Name);
        }

        public void testGenericMethod1()
        {
            var type = TypeOf<string>();
            AssertEquals("String", type.Name);
        }

        public void testGenericClass1()
        {
            var x = new GClass<string>();
            var type = x.TypeOfT();
            AssertEquals("String", type.Name);
        }

        public void testGenericClass2a()
        {
            var x = new GClass2<string>();
            var type = x.TypeOfT();
            AssertEquals("String", type.Name);
        }

        public void testGenericClass2b()
        {
            var x = new GClass2<string>();
            var type = x.TypeOfY<Java.Lang.Package>();
            AssertEquals("Package", type.Name);
        }

        public void testGenericClass3()
        {
            var x = new GClass3<string>();
            var type = x.TypeOfT();
            AssertTrue(type.IsArray);
            AssertEquals("String", type.GetElementType().Name);
        }

        public void testGenericClass4()
        {
            var x = new GClass4<string>();
            var type = x.TypeOfT();
            AssertTrue("dimension 1", type.IsArray);
            AssertTrue("dimension 2", type.GetElementType().IsArray);
            AssertEquals("String", type.GetElementType().GetElementType().Name);
        }
		
		public void testTypeOfVoid() 
		{
			var type = typeof(ReturnVoidClass);
			var method  = type.JavaGetDeclaredMethod("ReturnVoid");
			AssertTrue("void=void", method.ReturnType == typeof(void));
		}
		
		public void testTypeOfVoidAsync() 
		{
			var type = typeof(ReturnVoidClass);
			var method  = type.JavaGetDeclaredMethod("ReturnVoidAsync");
			AssertTrue("async void=void", method.ReturnType == typeof(void));
		}
		
		private class ReturnVoidClass
		{
			[Include]
			public void ReturnVoid() {}

			[Include]
			public async void ReturnVoidAsync() {}
		}

        private static Type TypeOf<T>()
        {
            return typeof (T);
        }

        private class GClass<T>
        {
            public Type TypeOfT()
            {
                return typeof (T);
            }
        }

        private class GClass2<T>
        {
            public Type TypeOfT()
            {
                return typeof(T);
            }

            public Type TypeOfY<Y>()
            {
                return typeof(Y);
            }
        }

        private class BaseClass<T>
        {
            public Type TypeOfT()
            {
                return typeof(T);
            }
        }

        private class GClass3<T> : BaseClass<T[]>
        {
        }

        private class GClass4<T> : BaseClass<T[,]>
        {
        }
    }
}
