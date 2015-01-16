using System.Diagnostics;
using Junit.Framework;

namespace Dot42.Tests.System.Diagnostics
{
    class TestStopwatch : TestCase
    {
        public void test1()
        {
            var sw = Stopwatch.StartNew();

            lock (this)
            {
                JavaWait(100);
            }

            sw.Stop();

            AssertFalse(sw.IsRunning);
            AssertTrue(sw.ElapsedMilliseconds >= 100 );
        }

        public void test2()
        {
            var sw = new Stopwatch();
            sw.Start();

            lock (this)
            {
                JavaWait(100);
            }

            sw.Stop();

            AssertFalse(sw.IsRunning);
            AssertTrue(sw.ElapsedMilliseconds >= 100);
        }
    }
}
