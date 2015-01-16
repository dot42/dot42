using Android.App;
using Android.Hardware;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 Orientation Sample")]

namespace Orientation
{
    [Activity(Icon = "Icon", ScreenOrientation = ScreenOrientations.Portrait)]
    public class MainActivity : Activity, ISensorEventListener
    {
        private Panel panel;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);

            var sensorManager = (SensorManager) GetSystemService(SENSOR_SERVICE);
            var sensors = sensorManager.GetSensorList(Sensor.TYPE_GRAVITY);
            if (sensors.Count > 0)
            {
                var sensor = sensors.Get(0);

                if (sensor != null)
                {
                    panel = new Panel(this);
                    SetContentView(panel);

                    sensorManager.RegisterListener(this, sensor, 0);
                    return;
                }
            }

            var text = new TextView(this) { Text = "No gravity sensor found" };
            SetContentView(text);
        }

        public void OnAccuracyChanged(Sensor sensor, int int32)
        {
        }

        public void OnSensorChanged(SensorEvent sensorEvent)
        {
            if (sensorEvent.Sensor.GetType() == Sensor.TYPE_GRAVITY)
            {
                double x = sensorEvent.Values[0];
                double y = sensorEvent.Values[1];
                //double z = sensorEvent.Values[2];

                panel.setOrientation(y, x);
            }
        }
    }
}