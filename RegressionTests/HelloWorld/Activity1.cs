using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace Dot42Application1
{
    [Activity(Label = "Activity1", VisibleInLauncher = false)]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
//            SetContentView(R.Layouts.Activity1Layout);
        }
    }
}
