using System;
using System.Threading;
using Junit.Framework;

namespace Dot42.Tests.System.Threading
{
    public class TestThread : TestCase
    {
        private static bool done;

        public void testThreadStart()
        {
            done = false;
            var t = new Thread(SetDone);
            t.Start();
            t.Join();
            AssertTrue(done);
        }

        private void SetDone()
        {
            done = true;
        }

    }
}
