using System;
using System.Diagnostics;
using Junit.Framework;

namespace Dot42.Tests.System.Diagnostics
{
    public class TestDebug : TestCase
    {
        public void testWrite1()
        {
            Debug.Write("test1");
        }

        //public void testWrite2()
        //{
        //    Debug.Write("test {0}", "MyCategory");
        //}

        public void testWriteLine1()
        {
            Debug.WriteLine("test1");
        }

        public void testWriteLine2()
        {
            Debug.WriteLine("test {0}", 2);
        }

        public void testAssert1()
        {
            Debug.Assert(true);
        }

        public void testAssert2()
        {
            try
            {
                Debug.Assert(false);
                Fail("Exception expected");
            }
            catch (Exception ex)
            {
                // Expected
            }
        }
    }
}
