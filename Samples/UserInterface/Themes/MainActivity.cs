using Android.App;
using Android.Os;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Using Themes Sample", Theme = "@android:style/Theme.Holo")]

namespace UsingThemes
{
    [Activity(Icon = "Icon", Label = "dot42 Using Themes!")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
        }
    }
}
