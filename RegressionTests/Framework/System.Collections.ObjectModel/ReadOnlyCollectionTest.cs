using System;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Dot42.Tests.System.Collections.ObjectModel
{
    [TestFixture]
    public class ReadOnlyCollectionTest
    {
        [Test]
        public void TestCtr()
        {
            var exceptions = new [] {new Exception("test")};

            var roc = new ReadOnlyCollection<Exception>(exceptions);

            Assert.AssertEquals(1, roc.Count);
        }
    }
}

