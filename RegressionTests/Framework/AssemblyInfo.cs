using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Framework Tests")]
[assembly: Instrumentation(Label = "dot42 Framework Tests", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]

[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_NETWORK_STATE)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_WIFI_STATE)]
[assembly: UsesPermission(Android.Manifest.Permission.CHANGE_WIFI_STATE)]