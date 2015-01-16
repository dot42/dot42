using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace UsingOuyaOdk.Sources
{
    [Activity(Label = "OUYA Test App")]
	[IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, 
				  Categories = new[] { "android.intent.category.LAUNCHER", "tv.ouya.intent.category.GAME" })]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
        }
    }
}
