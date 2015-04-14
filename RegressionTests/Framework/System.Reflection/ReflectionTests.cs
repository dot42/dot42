using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Dot42.Tests.System.Reflection
{
    [TestFixture]
    public class ReflectionTests
    {
        [Test]
        public void TestAssemblyOfType()
        {
            Assert.AreEqual("FrameworkTests", typeof(ReflectionTests).Assembly.FullName);
        }

        [Test]
        public void TestGetExecutingAssembly()
        {
            Assert.AreEqual(typeof(ReflectionTests).Assembly, Assembly.GetExecutingAssembly());
        }

        // these tests won't work, since on instrumentation, we might not be running on the main thread
        // at all!
        //[Test]
        //public void TestGetEntryAssembly()
        //{
        //    Assert.AreEqual(typeof(ReflectionTests).Assembly, Assembly.GetEntryAssembly());
        //}

        //[Test]
        //public void TestGetEntryAssemblyFromDifferentThread()
        //{
        //    global::System.Threading.Tasks.Task.Run(() =>
        //    {
        //        Assert.AreEqual(typeof (ReflectionTests).Assembly, Assembly.GetEntryAssembly());
        //    }).Wait();
        //}

        [Test]
        public void TestAssemblyGetTypes()
        {
            Assert.IsTrue(typeof(ReflectionTests).Assembly.DefinedTypes.Any(t => t.IsAssignableFrom(typeof(ReflectionTests).GetTypeInfo())));
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

        [Include]
        public class Task
        {
            public string Id { get; set; }
            public string FieldId;
        }

        [IncludeType]
        class WithEvent
        {
            public event EventHandler Event1;
            public event EventHandler<EventArgs> Event2;
        }

        [IncludeType]
        class WithPropertyChanged : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
        }

        [Test]
        public void TestEventInfo()
        {
            var events = typeof(WithEvent).GetEvents(BindingFlags.Instance | BindingFlags.Public);
            Assert.AreEqual(2, events.Length);
            Assert.IsTrue(events.Any(e => e.Name == "Event1"));
            Assert.IsTrue(events.Any(e => e.Name == "Event2"));
        }

        [Test]
        public void TestEventInfoFromInterface()
        {
            var events = typeof(WithPropertyChanged).GetEvents(BindingFlags.Instance | BindingFlags.Public);
            Assert.IsTrue(events.Any(e => e.Name == "PropertyChanged"));
            Assert.AreEqual(1, events.Length);
        }

        [Test]
        public void TestGetPublicNestedType()
        {
            var taskType = typeof(ReflectionTests).GetNestedType("Task");
            Assert.IsNotNull(taskType);
            Assert.AreEqual("Task", taskType.Name);
        }
#if NOT_IMPLEMENTED
        [Test]
        public void TestGetInternalNestedTypeWithoutBindingFlags()
        {
            var type = typeof(ReflectionTests).GetNestedType("WithEvent");
            Assert.IsNull(type);
        }
#endif

    }
}

