using Android.App;
using Android.Os;
using Dot42.Manifest;

[assembly: Application("dot42 Simple Fragment Sample")]

namespace SimpleFragment
{
    [Activity(Icon = "Icon", Label = "dot42 Simple Fragments!")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
        }
    }
}
