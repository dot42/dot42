using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Com.Google.Ads;

[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_NETWORK_STATE)]

namespace UsingGoogleMobAds.Sources
{
    [Activity(Label = "Using MobAds")]
    public class MainActivity : Activity
    {
		private AdView adView;
		private const string MY_AD_UNIT_ID = "dummy";
		
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
			SetContentView(R.Layouts.MainActivity);

			// Create the adView
			adView = new AdView(this, AdSize.BANNER, MY_AD_UNIT_ID);

			// Lookup your LinearLayout assuming it's been given
			// the attribute android:id="@+id/mainLayout"
			var layout = FindViewById<LinearLayout>(UsingGoogleMobAds.R.Ids.mainLayout);

			
			// Add the adView to it
			layout.AddView(adView);

			// Initiate a generic request to load it with an ad
			var adRequest = new AdRequest();
			adRequest.AddTestDevice("A3DD5CC21FB10074BE918999C41356D8"); // See adb logcat

			adView.LoadAd(adRequest);            
        }
    }
}
