using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            public T Value { get; set; }
            public List<T> FieldId;
        }

        [IncludeType]
        public class TaskDerived<T> :Task<T>
        {
            public T Value1 { get; set; }
            public List<T> FieldId1;
        }

        [IncludeType]
        class Task
        {
            public string Id { get; set; }
            public string FieldId;
        }

        [Include]
        public class Class1 { }
        [Include]
        public Task<int> intTask;
        [Include]
        public Task<DateTime> dateTimeTask;
        [Include]
        public Task<Class1> class1Task;

        [Include]
        public TaskDerived<Class1> class1TaskDerived;


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

        [Test]
        public void TestDeclaringTypeNonGeneric()
        {
            foreach (var m in typeof(Task).GetMethods())
                Console.WriteLine("Method '{0}' DeclaringType {1}", m.Name, m.DeclaringType.FullName);

            Assert.AreEqual(2, typeof(Task).GetDeclaredMethods().Count());
            Assert.AreEqual(typeof(object), typeof(Task).GetMethod("notify").DeclaringType);

        }

        [Test]
        public void TestDeclaringTypeGeneric()
        {
            foreach (var m in typeof(Task<>).GetMethods())
                Console.WriteLine("Method '{0}' DeclaringType {1}", m.Name, m.DeclaringType.FullName);

            Assert.AreEqual(2, typeof(Task<>).GetDeclaredMethods().Count());
            Assert.AreEqual(typeof(object), typeof(Task<>).GetMethod("notify").DeclaringType);

        }


        [Test]
        public void TestDeclaringTypeGenericDerived()
        {
            foreach (var m in typeof(TaskDerived<>).GetMethods())
                Console.WriteLine("Method '{0}' DeclaringType {1}", m.Name, m.DeclaringType.FullName);

            Assert.AreEqual(2, typeof(Task<>).GetDeclaredMethods().Count());
            Assert.AreEqual(typeof(object), typeof(Task<>).GetMethod("notify").DeclaringType);
        }

        [Test]
        public void TestDeclaringTypeGenericDerivedByField()
        {
            var type = typeof (GenericReflectionTest).GetField("class1TaskDerived").FieldType;
            foreach (var m in type.GetMethods())
                Console.WriteLine("Method '{0}' DeclaringType '{1}'", m.Name, m.DeclaringType.FullName);
            foreach (var m in type.GetFields())
                Console.WriteLine("Field '{0}' '{1}' DeclaringType '{2}'", m.FieldType.FullName, m.Name, m.DeclaringType.FullName);

            Assert.AreEqual(2, type.GetDeclaredMethods().Count());
            Assert.AreEqual(typeof(object), type.GetMethod("notify").DeclaringType);

        }


    }
}
