using Android.App;using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Using SeekBar Sample")]

namespace UsingSeekBar
{
    [Activity(Icon = "Icon", Label = "dot42 Using SeekBar!")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layout.MainLayout);

            var myBar = FindViewById<SeekBar>(R.Id.myBar);
            var myLabel = FindViewById<TextView>(R.Id.myLabel);
            myBar.ProgressChanged += (s, x) => myLabel.Text = x.Progress.ToString();
        }
    }
}
