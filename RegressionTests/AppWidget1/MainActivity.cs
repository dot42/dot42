using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("AppWidget1")]

namespace AppWidget1
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layout.MainLayout);
        }
    }
}
