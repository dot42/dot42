using System;
using System.ComponentModel;
using System.Threading;
using Android.OS;
using Junit.Framework;

namespace Dot42.Tests.System.ComponentModel
{
    public class TestBackgroundWorker : TestCase
    {
        public void testStaticAction1()
        {
            var testThread = new TestThread();
            testThread.Start();
            Thread.Sleep(1000);
            testThread.Quit();
            AssertNull("Error should be null", testThread.error);
            AssertEquals("Result should be OK", "OK", testThread.result);
        }

        private class TestThread : HandlerThread 
        {
            public string result = null;
            public object error = "dummy";

            public TestThread()
                : base("testBackgroundWorker")
            {                
            }

            protected internal override void OnLooperPrepared()
            {
                var worker = new BackgroundWorker();
                worker.DoWork += WorkerOnDoWork;
                worker.RunWorkerCompleted += (s, x) => {
                    error = x.Error;
                    result = (string)x.Result;
                };
                worker.RunWorkerAsync();
            }

            private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
            {
                e.Result = "OK";
            }
        }
    }
}
