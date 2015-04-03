using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Android.Util;
using NUnit.Framework;

namespace Dot42.Tests.System.Reflection
{
    [TestFixture]
    public class ReflectionInfoEqualityTest
    {
        [Include]
        class Task
        {
            public string Id { get; set; }
            public string FieldId;
        }

        public void TestFieldEquality()
        {
            var task = new Task();
            var task2 = new Task();
            Type t = task.GetType();
            Type t2 = task2.GetType();

            var prop1 = t.GetProperty("FieldId");
            var prop2 = t2.GetProperty("FieldId");

            Assert.AreEqual(prop1, prop2);
            // this fails:
            //Assert.AreSame(prop1, prop2);
        }

        public void TestPropertyEquality()
        {
            var task = new Task();
            var task2 = new Task();
            Type t = task.GetType();
            Type t2 = task2.GetType();

            var prop1 = t.GetProperty("Id");
            var prop2 = t2.GetProperty("Id");

            Assert.AreEqual(prop1, prop2);
            // this fails:
            //Assert.AreSame(prop1, prop2);
        }

        
    }
}
