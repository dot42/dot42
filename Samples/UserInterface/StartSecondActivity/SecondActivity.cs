using Android.App;
using Dot42;
using Dot42.Manifest;

namespace StartSecondActivity
{
    [Activity(VisibleInLauncher=false)]
    public class SecondActivity : Activity
    {
        protected override void OnCreate(Android.Os.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.SecondActivity);
        }
    }
}
