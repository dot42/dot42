using System;
using Android.App;using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42SipTester1")]

namespace dot42SipTester1
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);

            bool isOk = clsSIPTest.RegisterExt("2000");





            SetContentView(R.Layout.MainLayout);
        }
    }
}
