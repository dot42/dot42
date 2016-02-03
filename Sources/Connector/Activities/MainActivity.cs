using Android.App;
using Android.OS;
using Dot42;
using Dot42.Manifest;

[assembly: Application("Dot42 Connector")]

namespace Dot42.Connector.Activities
{
    [Activity(Icon = "Icon", Label = "Hello World!")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layout.MainLayout);
        }
    }
}
