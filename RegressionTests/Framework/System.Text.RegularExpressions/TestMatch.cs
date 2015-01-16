using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Dot42.Tests.System.Text.RegularExpressions
{
    [TestFixture]
    class TestMatch
    {


        [Test]
        [NUnit.Framework.Ignore]
        public void NameLookupInEmptyMatch()
        {
            Regex regTime = new Regex(
                    @"(?<hour>[0-9]{1,2})([\:](?<minute>[0-9]{1,2})){0,1}([\:](?<second>[0-9]{1,2})){0,1}\s*(?<ampm>(?i:(am|pm)){0,1})");

            Match mTime = regTime.Match("");
            Assert.AreEqual("", mTime.Groups["hour"].Value, "#A1");
            Assert.AreEqual("", mTime.Groups["minute"].Value, "#A2");
            Assert.AreEqual("", mTime.Groups["second"].Value, "#A3");
            Assert.AreEqual("", mTime.Groups["ampm"].Value, "#A4");
        }

        [Test]
        public void NameLookupMatch()
        {
            Regex regTime = new Regex(
                    @"(?<hour>[0-9]{1,2})([\:](?<minute>[0-9]{1,2})){0,1}([\:](?<second>[0-9]{1,2})){0,1}\s*(?<ampm>(?i:(am|pm)){0,1})");

            Match mTime = regTime.Match("12:00 pm");

            Assert.AreEqual(4, regTime.GroupNumberFromName("hour"), "#A1");
            Assert.AreEqual(5, regTime.GroupNumberFromName("minute"), "#A1");
            Assert.AreEqual(6, regTime.GroupNumberFromName("second"), "#A1");
            Assert.AreEqual(7, regTime.GroupNumberFromName("ampm"), "#A1");

            Assert.AreEqual("12", mTime.Groups["hour"].Value, "#B1");
            Assert.AreEqual("00", mTime.Groups["minute"].Value, "#B2");
            Assert.AreEqual("", mTime.Groups["second"].Value, "#B3");
            Assert.AreEqual("pm", mTime.Groups["ampm"].Value, "#B4");
        }

        [Test]
        public void IndexMatch()
        {
            Regex regTime = new Regex(
                    @"([0-9]{1,2})([\:]([0-9]{1,2})){0,1}([\:]([0-9]{1,2})){0,1}\s*((?i:(am|pm)){0,1})");

            Match mTime = regTime.Match("12:00 pm");
            Assert.AreEqual(8, mTime.Groups.Count, "#A0");
            Assert.AreEqual("12:00 pm", mTime.Groups[0].Value, "#B0");
            Assert.AreEqual("12", mTime.Groups[1].Value, "#B1");
            Assert.AreEqual(":00", mTime.Groups[2].Value, "#B2");
            Assert.AreEqual("00", mTime.Groups[3].Value, "#B3");
            Assert.AreEqual("", mTime.Groups[4].Value, "#B4");
            Assert.AreEqual("", mTime.Groups[5].Value, "#B5");
            Assert.AreEqual("pm", mTime.Groups[6].Value, "#B6");
            Assert.AreEqual("pm", mTime.Groups[7].Value, "#B7");
        }

        [Test]
        public void MatchSimple1()
        {
            Regex regTime = new Regex(@"Java");

            Match mTime = regTime.Match("This is Java 7");
            Assert.AreEqual(1, mTime.Groups.Count, "#A1");
            Assert.AreEqual("Java", mTime.Groups[0].Value, "#A2");
            Assert.AreEqual(8, mTime.Index, "#A3");
            Assert.AreEqual(4, mTime.Length, "#A4");
        }

        [Test]
        public void MatchSimple2()
        {
            Regex regTime = new Regex(@"Java\s*([0-9]{1,2})");

            Match mTime = regTime.Match("This is Java 7");
            Assert.AreEqual(2, mTime.Groups.Count, "#A1");
            Assert.AreEqual("Java 7", mTime.Groups[0].Value, "#A2");
            Assert.AreEqual(8, mTime.Index, "#A3");
            Assert.AreEqual(6, mTime.Length, "#A4");

            Assert.AreEqual("7", mTime.Groups[1].Value, "#A5");
        }
    }
}
