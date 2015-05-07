using System;
using Android.App;using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Com.Google.Android.Gms.Common;

namespace UsingSuportLibrary.Sources
{
    [Activity(Label = "MainActivity")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            // Calling this method triggers a validation error.
         Android.Content.Context context = GetApplicationContext();
         if (null != context)
         {
            int result = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(context);
            if (ConnectionResult.SUCCESS != result)
            {
               switch (result)
               {
                  case ConnectionResult.SERVICE_MISSING:
                     break;
               }
            }
         }
            
        }
    }
}
