using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Android.Provider;
using NUnit.Framework;

namespace Dot42.Tests.System.Reflection
{
    [TestFixture]
    public class EventInfoTest
    {
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
            var events = typeof (WithEvent).GetEvents(BindingFlags.Instance | BindingFlags.Public);
            Assert.AreEqual(2, events.Length);
            Assert.IsTrue(events.Any(e=>e.Name == "Event1"));
            Assert.IsTrue(events.Any(e => e.Name == "Event2"));
        }

        [Test]
        public void TestEventInfoFromInterface()
        {
            var events = typeof(WithPropertyChanged).GetEvents(BindingFlags.Instance | BindingFlags.Public);
            Assert.IsTrue(events.Any(e => e.Name == "PropertyChanged"));
            Assert.AreEqual(1, events.Length);
        }
    }
}
