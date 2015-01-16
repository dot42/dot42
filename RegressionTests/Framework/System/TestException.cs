using System;
using Android.Util;
using Junit.Framework;

namespace Dot42.Tests.System
{
    public class TestException : TestCase
    {
        public void testStackTrace()
        {
            try
            {
                throw new InvalidCastException("Test");
            }
            catch (Exception ex)
            {
                string st = ex.StackTrace;

                AssertNotNull(st);
                Log.D("Exception", st);
            }
        }

    }
}
