using System;
using System.Globalization;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestByte : TestCase
    {
        public void testCompareTo1()
        {
			byte a = 5;
			byte b = 6;
			var x = a.CompareTo(b);
			AssertTrue(x < 0);
        }

        public void testCompareTo2()
        {
            byte a = 5;
            byte b = 130;
            var x = a.CompareTo(b);
            AssertTrue(x < 0);
        }

        public void testCompareTo3()
        {
            byte a = 5;
            byte b = 255;
            var x = a.CompareTo(b);
            AssertTrue(x < 0);
        }

        public void testToString()
        {
            AssertEquals("5", ((byte)5).ToString(CultureInfo.InvariantCulture));
            AssertEquals("255", ((byte)255).ToString(CultureInfo.InvariantCulture));
            AssertEquals("254", ((byte)254).ToString(CultureInfo.InvariantCulture));
        }
    }
}
