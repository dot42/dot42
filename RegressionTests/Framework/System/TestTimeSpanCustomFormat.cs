using System;
using System.Globalization;
using NUnit.Framework;

namespace Dot42.Tests.System
{
    [TestFixture]
    public class TestTimeSpanCustomFormat 
    {
        [Test]
        public void TestCustomFormats()
        {
            Assert.AreEqual("01:02", new TimeSpan(0, 1, 2).ToString("mm':'ss",CultureInfo.InvariantCulture), "#A01");
            Assert.AreEqual("3:01", new TimeSpan(3, 1, 2).ToString("h':'mm", CultureInfo.InvariantCulture), "#A02");
        }
    }
}
