using System;
using System.Net;
using System.Threading;

using Android.App;
using Android.Graphics;
using Android.Os;
using Android.Widget;

using Dot42.Manifest;


[assembly: Application("dot42AppNUnit")]
[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]

namespace dot42AppNUnit
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            SynchronizationContext.SetSynchronizationContext(this);

            DownloadImageAsync();
        }

        private async void DownloadImageAsync()
        {
            try
            {
                var webClient = new WebClient();
                var data = await webClient.DownloadDataTaskAsync("http://www.dot42.com/dot42/img/logo.png").ConfigureAwait(this);

                var textView = FindViewById<TextView>(R.Ids.MyText);

                var bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                if (bitmap == null)
                {
                    textView.SetText("BitmapFactory cannot convert bytes into Bitmap");
                }
                else
                {
                    textView.SetText("Ready:");
                    var imageView = FindViewById<ImageView>(R.Ids.MyImage);
                    imageView.SetImageBitmap(bitmap);
                }
            }
            catch (Exception ex)
            {
                var textView = FindViewById<TextView>(R.Ids.MyText);
                textView.SetText("Exception: " + ex.Message);
            }
        }
    }
}
