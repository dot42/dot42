using Android.App;
using Android.Os;
using Android.Widget;
using Android.Location;
using Dot42.Manifest;

[assembly: Application("dot42 Simple GPS sample", Icon = "Icon")]

// these replace similar entries in AndroidManifest.xml
[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_FINE_LOCATION)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_COARSE_LOCATION)]

namespace SimpleGps
{
    [Activity]
    public class MainActivity : Activity, ILocationListener /* implements LocationListener */
    {
        private LocationManager service;
        private TextView txStatus;
        private TextView txProvider;
        private TextView txAccuracy;
        private TextView txLatitude;
        private TextView txLongtitude;
        private TextView txAltitude;
        private TextView txSpeed;
        private TextView txExtra;
        private string provider;
        private bool enabled;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            // Find UI controls
            txStatus = FindViewById<TextView>(R.Ids.txStatus);
            txProvider = FindViewById<TextView>(R.Ids.txProvider);
            txAccuracy = FindViewById<TextView>(R.Ids.txAccuracy);
            txLatitude = FindViewById<TextView>(R.Ids.txLatitude);
            txLongtitude = FindViewById<TextView>(R.Ids.txLongtitude);
            txAltitude = FindViewById<TextView>(R.Ids.txAltitude);
            txSpeed = FindViewById<TextView>(R.Ids.txSpeed);
            txExtra = FindViewById<TextView>(R.Ids.txExtra);

            // capitalize getSystemService
            service = (LocationManager)GetSystemService(LOCATION_SERVICE);

            enabled = service.IsProviderEnabled(LocationManager.GPS_PROVIDER);
            if (enabled)
            {
                txStatus.Text = "GPS enabled";
            }
            else
            {
                txStatus.Text = "GPS not enabled - change settings first and come back";
                return;
            }

            var criteria = new Criteria { Accuracy = Criteria.ACCURACY_FINE };
            provider = service.GetBestProvider(criteria, false);
            var location = service.GetLastKnownLocation(provider);
            if (null != location)
            {
                txProvider.Text = "Using provider '" + provider + "'";
                OnLocationChanged(location);
            }
            else
            {
                txProvider.Text = "No location";
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (enabled) service.RequestLocationUpdates(
                provider,
                1000, // minimum time interval between updates in ms
                1, // minimum distance between updates in m
                this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (enabled) service.RemoveUpdates(this);
        }

        #region implement ILocationListener

        public void OnLocationChanged(Location location)
        {
            var latitude = location.Latitude;
            var longtitude = location.Longitude;
            var altitude = location.Altitude;
            var speed = location.Speed;
            var satellites = (location.Extras != null) ? location.Extras.GetInt("satellites") : 0;

            txAccuracy.Text = "Accuracy: " + location.GetAccuracy() + " m";
            txLatitude.Text = "Latitude: " + latitude + " degrees";
            txLongtitude.Text = "Longtitude: " + longtitude + " degrees";
            txAltitude.Text = "Altitude: " + altitude + " m";
            txSpeed.Text = "Speed: " + speed + " m/s";
            txExtra.Text = "#Satellites: " + satellites;
        }

        public void OnStatusChanged(string provider, int status, Bundle bundle)
        {
        }

        public void OnProviderEnabled(string provider)
        {
            txProvider.Text = "Enabled new provider " + provider;
        }

        public void OnProviderDisabled(string provider)
        {
            txProvider.Text = "Disabled provider " + provider;
        }

        #endregion
    }
}
