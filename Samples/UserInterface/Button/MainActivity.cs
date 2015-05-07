using Android.App;using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Using Button Sample")]

namespace UsingButton
{
    [Activity(Icon = "Icon", Label = "dot42 Using Button!")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layout.MainLayout);

            var myButton = (Button)FindViewById(R.Id.myButton);
            myButton.Click += (s, x) => myButton.Text = "I'm clicked";
        }
    }
}
