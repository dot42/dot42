using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 Alert Dialog")]

namespace Alert
{
    [Activity(Icon = "Icon")]
    public class MainActivity : Activity
    {
        /// <summary>
        /// Activity is created.
        /// </summary>
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
            var button = FindViewById<Button>(R.Ids.button);
            button.Click += ButtonOnClick;
        }

        private void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage("This is an Alert Dialog.");
            var dialog = builder.Create();
            dialog.Show();
        }
    }
}
