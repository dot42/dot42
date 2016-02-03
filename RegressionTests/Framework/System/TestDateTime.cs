using System;
using System.Globalization;
using NUnit.Framework;

namespace Dot42.Tests.System
{
    [TestFixture]
    public class TestDateTime
    {
        [Test]
        public void TestCtor1()
        {
            DateTime x = new DateTime();
            Assert.AreEqual(0, x.Ticks);
        }

        [Test]
        public void TestCtor2()
        {
            var now = new DateTime(123456789);
            Assert.AreEqual(123456789L, now.Ticks);
        }

        [Test]
        public void TestMinValueParseRoundtrip()
        {
            const string pattern = "yyyyMMdd'T'HHmm'Z'";
            
            var minValueString = DateTime.MinValue.ToString(pattern, CultureInfo.InvariantCulture);

            Assert.AreEqual("00010101T0000Z",minValueString);
            Assert.AreEqual(DateTime.MinValue, DateTime.ParseExact(minValueString, pattern, CultureInfo.InvariantCulture));
        }

        [Test]
        public void Test()
        {
            var time = DateTime.Now;
            while (true)
            {
                var timeSoFar = new TimeSpan(DateTime.Now.Ticks - time.Ticks);
                if (timeSoFar.TotalMilliseconds > 10)
                {
                    break;
                }
            }
        }

        [Test]
        public void TestCase860()
        {
            long l = 635119746585970000L;

            DateTime dateTime = new DateTime(l);

            var str = dateTime.ToString("dd.MM.yyyy H:mm");
            Assert.AreEqual("13.08.2013 7:10", str);

            var sd = dateTime.ToString("d", CultureInfo.InvariantCulture);
            Assert.AreEqual("08/13/2013", sd);
        }

    }

}
