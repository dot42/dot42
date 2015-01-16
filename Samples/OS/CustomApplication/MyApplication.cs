using Android.App;
using Android.Util;
using Dot42.Manifest;

namespace CustomApplication
{
    [Application("Custom application class", Icon = "Icon")]
    public class MyApplication : Application
    {
        private const string Tag = "CustomApplication";

        public override void OnCreate()
        {
            base.OnCreate();
            Log.I(Tag, "MyApplication.OnCreate");
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            Log.I(Tag, "MyApplication.OnConfigurationChanged");
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            Log.I(Tag, "MyApplication.OnLowMemory");
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            Log.I(Tag, "MyApplication.OnTerminate");
        }
    }
}
