using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;using Android.OS;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("Dot42RegressionTestService")]

namespace ServiceTest
{
    [Service(Label = "Dot42RegressionTestService")]
    public class TestService : Service
    {
        private ServiceHandler handler;

        public override IIBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            var thread = new HandlerThread("RegressionTestService", Process.THREAD_PRIORITY_BACKGROUND);
            thread.Start();
            handler = new ServiceHandler(thread.Looper);
        }

        public override int OnStartCommand(Intent intent, int flags, int startId)
        {
            Toast.MakeText(this, "Service starting", Toast.LENGTH_LONG).Show();
            var m = handler.ObtainMessage();
            m.Arg1 = startId;
            handler.SendMessage(m);
            return START_STICKY;
        }

        public override void OnDestroy()
        {
            Toast.MakeText(this, "Service stopping", Toast.LENGTH_LONG).Show();
        }

        private class ServiceHandler : Handler
        {
            public ServiceHandler(Looper looper)
                : base(looper)
            {                
            }

            public override void HandleMessage(Message msg)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
