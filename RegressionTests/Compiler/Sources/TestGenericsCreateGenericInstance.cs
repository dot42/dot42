using System;
using System.Collections;
using System.Collections.Generic;
using Junit.Framework;

namespace Dot42.Tests.Compiler.Sources
{
    public class TestGenericsCreateGenericInstance: TestCase
    {
        public void testGenericTypeDefinition()
        {
            var genericType = typeof (Dictionary<,>);

            AssertTrue(genericType.IsGenericTypeDefinition);
            AssertEquals(2, genericType.GetGenericArguments().Length);
            AssertTrue(genericType.ContainsGenericParameters);
        }

        public void testMakeGenericTypes()
        {
            var genericType = typeof(Dictionary<,>);

            var type1 = genericType.MakeGenericType(typeof (int), typeof (string));
            var type2 = genericType.MakeGenericType(typeof(string), typeof(int));
            
            AssertNotSame(type1, type2);
        }

        public void testMakeGenericInstance()
        {
            var genericType = typeof(Dictionary<,>);

            var type = genericType.MakeGenericType(typeof(int), typeof(string));

            var obj = Activator.CreateInstance(type);

            AssertTrue(obj is IDictionary);
            AssertTrue(obj is IDictionary<int, string>);
        }

        public void testCorrectGenericInstance()
        {
            var genericType = typeof(Dictionary<,>);

            var type = genericType.MakeGenericType(typeof(int), typeof(string));

            var dict = (IDictionary)Activator.CreateInstance(type);
            dict.Add(1, "Test1");
            
            AssertEquals("Test1", dict[1]);

            try
            {
                dict["invalid"] = 1;
                Fail("Dictionary<int,string> should not accept string as key");
            }
            catch (ArgumentException)
            {
            }
#if NOT_IMPLEMENTED
            try
            {
                dict[1] = 1;
                Fail("Dictionary<int,string> should not accept int as value");
            }
            catch (ArgumentException)
            {
            }
#endif
        }

    }
}
