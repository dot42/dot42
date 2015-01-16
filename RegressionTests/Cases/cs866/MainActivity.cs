using System;
using System.Collections;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using NUnit.Framework;

[assembly: Application("dot42App7")]

namespace dot42App7
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var s = string.Format("{0}", 42);
        }
    }
}
