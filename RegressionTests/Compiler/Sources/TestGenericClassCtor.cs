using System;
using Java.Lang;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericClassCtor : TestCase
    {
        public void testCtor1()
        {
            var type = GClass<string, Package>.Array.GetType();
            AssertEquals("Object", type.GetElementType().Name); // Not really what you expect, but that's what we got 
        }

        private class GClass<T1, T2>
        {
            public static readonly T1[] Array = new T1[1];
        }
    }
}
