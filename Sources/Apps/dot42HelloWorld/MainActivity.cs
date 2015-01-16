using Android.App;
using Android.Os;
using Android.Text;
using Android.Text.Method;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 Hello World", Icon = "Icon")]
[assembly: Package(VersionName = "1.0.0", VersionCode = 2)]

namespace dot42HelloWorld
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
            var view = FindViewById<TextView>(R.Ids.textView);
            view.Text = Html.FromHtml("<p>This is a simple 'Hello world' written in C# using dot42.</p>" +
                "<p>Go to <a href='http://www.dot42.com?ref=631'>www.dot42.com</a> for more information.</p>");
            view.MovementMethod = LinkMovementMethod.GetInstance();
        }
    }
}
