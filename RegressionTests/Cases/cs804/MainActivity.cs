using System;
using System.Collections;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using NUnit.Framework;
using System.Threading;

[assembly: Application("dot42_cs804")]

namespace dot42.cs804
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
			var x = new System.OperationCanceledException(new CancellationToken());
        }
    }
}
