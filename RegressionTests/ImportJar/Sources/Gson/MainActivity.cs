using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace ImportJar.Sources.Gson
{
    [Activity(Label = "Using GSon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
			SetContentView(R.Layouts.GsonActivity);
        }
    }
}
