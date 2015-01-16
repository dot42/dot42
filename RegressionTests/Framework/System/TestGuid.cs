using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Dot42.Tests.System
{
    [TestFixture]
    public class TestGuid
    {
        [Test]
        public void TestCtor()
        {
            string s = Guid.NewGuid().ToString();
            var guid = new Guid(s);

            Assert.AreEqual(s, guid.ToString());
        }
    }
}
