using System;
using System.Globalization;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestUint16 : TestCase
    {
        public void testCompareTo1()
        {
            UInt16 a = 5;
            UInt16 b = 6;
            var x = a.CompareTo(b);
            AssertTrue(x < 0);
        }

        public void testCompareTo2()
        {
            UInt16 a = 5;
            UInt16 b = 0xFFFF;
            var x = a.CompareTo(b);
            AssertTrue(x < 0);
        }

        public void testToString()
        {
            AssertEquals("5", ((ushort)5).ToString(CultureInfo.InvariantCulture));
            AssertEquals("65535", ((ushort)65535).ToString(CultureInfo.InvariantCulture));
        }
    }
}
