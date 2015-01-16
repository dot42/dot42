using System;
using Android.App;
using Android.Content;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;
using Uri = Android.Net.Uri;

[assembly: Application("dot42 File Browser Sample")]

namespace FileBrowser
{
    [Activity(Icon = "Icon", Label = "dot42 File Browser Sample")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var button = FindViewById<Button>(R.Ids.cmdOpen);
            button.Click += new System.EventHandler(OnOpenClick);

            var view = FindViewById<TextView>(R.Ids.txtInfo);
            view.Text = "Click open to browse for a file";
        }

        void OnOpenClick(object sender, System.EventArgs e)
        {
            StartActivityForResult(new Intent(this, typeof(OpenFileActivity)), 23);
        }

        protected override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            var view = FindViewById<TextView>(R.Ids.txtInfo);
            if (resultCode == RESULT_OK)
            {
                var path = data.GetStringExtra(OpenFileActivity.ResultPath);
                view.Text = path;

                try
                {
                    var intent = new Intent(Intent.ACTION_VIEW);
                    intent.SetData(Uri.FromFile(new Java.Io.File(path)));
                    StartActivity(intent);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "Failed to open file: " + ex.Message, Toast.LENGTH_LONG).Show();
                }
            }
            else
            {
                view.Text = string.Format("Result code is {0}", resultCode);
            }
        }
    }
}
