using System;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestConvert : TestCase
    {
        public void testToBase64a()
        {
			var bytes = new[] { (byte)'A', (byte)'B' };
			var base64 = Convert.ToBase64String(bytes);
            AssertEquals("QUI=", base64);
        }

        public void testToBase64b()
        {
			var base64 = Convert.ToBase64String(global::System.Text.Encoding.UTF8.GetBytes("aap"));
            AssertEquals("YWFw", base64);
        }

        public void testToInt64()
        {
			string bin = "10010101010101"; 
			long l = Convert.ToInt64(bin, 2);
            AssertEquals(0x2555L, l);
        }
	}
}
