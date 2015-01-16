using System;
using Android.App;
using Android.Hardware;
using Android.Os;
using Android.Util;
using Android.View;
using Android.Widget;
using Dot42.Manifest;
using Java.Io;
using Java.Text;
using Java.Util;
using Environment = Android.Os.Environment;

[assembly: Application("dot42 Simple Camera", Icon = "Icon")]

[assembly: UsesFeature("android.hardware.camera")]
[assembly: UsesPermission(Android.Manifest.Permission.CAMERA)]
[assembly: UsesPermission(Android.Manifest.Permission.WRITE_EXTERNAL_STORAGE)]

namespace SimpleCamera
{
    /// <summary>
    /// Demonstrates how to preview the camera and take pictures with it.
    /// 
    /// This sample was inspired by: http://stackoverflow.com/questions/10913682/how-to-capture-and-save-an-image-using-custom-camera-in-android
    /// </summary>
    [Activity(ScreenOrientation = ScreenOrientations.Landscape)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var camera = GetCamera();
            var preview = FindViewById<FrameLayout>(R.Ids.preview);
            var captureButton = FindViewById<Button>(R.Ids.captureButton);
            var previewButton = FindViewById<Button>(R.Ids.previewButton);

            if (camera != null)
            {
                var cameraPreview = new CameraPreview(this, camera);
                preview.AddView(cameraPreview);
                previewButton.Visibility = View.INVISIBLE;
                captureButton.Click += (s, x) => {
                    camera.TakePicture(null, null, new PictureCallback());
                    previewButton.Visibility = View.VISIBLE;
                };
                previewButton.Click += (s, x) => {
                    camera.StartPreview();
                    previewButton.Visibility = View.INVISIBLE;
                };
            }
            else
            {
                preview.AddView(new TextView(this) { Text = "No camera found" });
                captureButton.Visibility = View.INVISIBLE;
                previewButton.Visibility = View.INVISIBLE;
            }
        }

        /// <summary>
        /// Open the camera or return null on errors.
        /// </summary>
        private static Camera GetCamera()
        {
            try
            {
                return Camera.Open();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Callback for TakePicture.
        /// </summary>
        private class PictureCallback : Camera.IPictureCallback
        {
            public void OnPictureTaken(sbyte[] data, Camera camera)
            {
                var pictureFile = GetOutputMediaFile();
                if (pictureFile == null)
                {
                    return;
                }
                try
                {
                    var fos = new FileOutputStream(pictureFile);
                    fos.Write(data);
                    fos.Close();
                }
                catch
                {
                    // Ignore for now
                }
            }

            /// <summary>
            /// Gets a file where to store the picture.
            /// </summary>
            private static File GetOutputMediaFile()
            {
                var mediaStorageDir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DIRECTORY_PICTURES), "dot42 Simple Camera");
                if (!mediaStorageDir.Exists())
                {
                    if (!mediaStorageDir.Mkdirs())
                    {
                        Log.E("dot42 Simple Camera", "failed to create directory");
                        return null;
                    }
                }
                // Create a media file name
                var timeStamp = new SimpleDateFormat("yyyyMMdd_HHmmss").Format(new Date());
                var mediaFile = new File(mediaStorageDir.Path + File.Separator + "IMG_" + timeStamp + ".jpg");
                return mediaFile;
            }
        }
    }
}
