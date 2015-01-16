using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Android.Support.V4.App;

namespace UsingSuportLibrary.Sources
{
    [Activity(Label = "FragmentDialogActivity")]
    public class FragmentDialogActivity : FragmentActivity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.FragmentDialogActivity_Layout);
        }
    }
}
