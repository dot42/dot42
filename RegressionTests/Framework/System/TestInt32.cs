using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestInt32 : TestCase
    {
        public void testCompareTo1()
        {
			int a = 5;
			int b = 6;
			var x = a.CompareTo(b);
			AssertTrue(x < 0);
        }

        public void testTryParse1()
        {
            int x;
            var result = int.TryParse("10", out x);
            AssertTrue(result);
            AssertEquals(10, x);
        }

        public void testTryParse2()
        {
            var parts = new[] { "aap", "15" };
            AssertEquals(-1, CallTryParse(parts, 0));
            AssertEquals(15, CallTryParse(parts, 1));
        }

        private static int CallTryParse(string[] parts, int index)
        {
            int x;
            if (int.TryParse(parts[index], out x))
                return x;
            return -1;
        }

        public void testTryParseMany1()
        {
            var parts = new[] { "10", "15" };
            int x, y = 0;
            var result = int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y);
            AssertTrue(result);
            AssertEquals(10, x);
            AssertEquals(15, y);
        }

        public void testTryParseMany2()
        {
            var parts = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };
            int typeID = 0;
            int mask = 0;
            int errorMask = 0;
            int signedVal = 0;
            int factor = 0;
            int offset = 0;
            int decPoint = 0;
            int minValue = 0;
            int maxValue = 0;
            int opMode = 0;

            if (Int32.TryParse(parts[0], out typeID) &&
                   Int32.TryParse(parts[1], out mask) &&
                   Int32.TryParse(parts[2], out errorMask) &&
                   Int32.TryParse(parts[3], out signedVal) &&
                   Int32.TryParse(parts[4], out factor) &&
                   Int32.TryParse(parts[5], out offset) &&
                   Int32.TryParse(parts[6], out decPoint) &&
                   Int32.TryParse(parts[7], out minValue) &&
                   Int32.TryParse(parts[8], out maxValue) &&
                   Int32.TryParse(parts[10], out opMode)
                   )
            {
                AssertTrue(true);
            }
            else
            {
                AssertTrue(false);
            }
        }

    }
}
