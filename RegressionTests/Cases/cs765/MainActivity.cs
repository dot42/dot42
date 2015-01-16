using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

using Com.Parse;

[assembly: Application("dot42parse")]

namespace dot42parse
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            Parse.Initialize(this, "hsVJWZFJfXHryDiaJmeu4O9eRJHmMObFeapUqUMA", "swAFyVV0mnRZ5D1xU1NTe7yzQH5jvQASvGZd1Yxw");

            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
        }
    }
}
