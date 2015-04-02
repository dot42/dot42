using System;
using System.Collections.Generic;
using Android.Util;
using NUnit.Framework;

namespace Dot42.Tests.System.Reflection
{
    [TestFixture]
    public class GenericReflectionTest
    {
        [IncludeType]
        public class Task<T>
        {
            public T Value;
            public List<T> FieldId;
        }

        [Include]
        public class Class1 { }
        [Include]
        public Task<int> intTask;
        [Include]
        public Task<DateTime> dateTimeTask;
        [Include]
        public Task<Class1> class1Task;

        [Test]
        public void TestGenericFieldTypesInt()
        {
            var intType = typeof(GenericReflectionTest).GetField("intTask").FieldType;
            Log.I("dot42", intType.FullName);
            Assert.IsTrue(intType.IsGenericType);
            Assert.AreEqual(1, intType.GetGenericArguments().Length);
            Assert.AreEqual(typeof(Task<>), intType.GetGenericTypeDefinition());
            Assert.AreEqual(typeof(int), intType.GetGenericArguments()[0]);
        }

        [Test]
        public void TestGenericFieldTypesDateTime()
        {
            var dateTimeType = typeof(GenericReflectionTest).GetField("dateTimeTask").FieldType;
            Log.I("dot42", dateTimeType.FullName);
            Assert.IsTrue(dateTimeType.IsGenericType);
            Assert.AreEqual(1, dateTimeType.GetGenericArguments().Length);
            Assert.AreEqual(typeof(Task<>), dateTimeType.GetGenericTypeDefinition());
            Assert.AreEqual(typeof(DateTime), dateTimeType.GetGenericArguments()[0]);
        }

        [Test]
        public void TestGenericFieldTypesClass()
        {
            var class1Type = typeof(GenericReflectionTest).GetField("class1Task").FieldType;
            Log.I("dot42", class1Type.FullName);
            Assert.IsTrue(class1Type.IsGenericType);
            Assert.AreEqual(1, class1Type.GetGenericArguments().Length);
            Assert.AreEqual(typeof(Task<>), class1Type.GetGenericTypeDefinition());
            Assert.AreEqual(typeof(Class1), class1Type.GetGenericArguments()[0]);
        }


    }
}
