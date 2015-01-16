using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Intent = Android.Content.Intent;

[assembly: Application("dot42Manifest")]

namespace dot42Manifest
{
    [Activity]
    [IntentFilter(Actions = new[] { Intent.ACTION_ANSWER }, Categories = new[] { Intent.CATEGORY_DEFAULT })]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
        }
    }
}
