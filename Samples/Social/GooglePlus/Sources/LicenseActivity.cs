using System;

using Dot42.Manifest;

using Android.App;
using Android.View;
using Android.Widget;

using Com.Google.Android.Gms.Common;

namespace GooglePlusClient
{
   [Activity(VisibleInLauncher = false)]
   class LicenseActivity : Activity
   {
      protected override void OnCreate(Android.Os.Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);

         ScrollView scroll = new ScrollView(this);
         TextView license = new TextView(this);
         license.SetText(GooglePlayServicesUtil.GetOpenSourceSoftwareLicenseInfo(this));
         scroll.AddView(license);
         SetContentView(scroll);
      }
   }
}
