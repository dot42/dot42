using Android.App;
using Android.Content;using Android.OS;using Android.Views;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Start Second Activity", Icon = "@drawable/Icon")]

namespace StartSecondActivity
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layout.MainLayout);
        }

        [Include]
        public void OnStartSecond(View sender)
        {
            var intent = new Intent(this, typeof (SecondActivity));
            StartActivity(intent);
        }
    }
}
