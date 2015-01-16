using Dot42.Manifest;

[assembly: Application("KiloBoltRobotGame", Icon = "icon")]

[assembly: UsesPermission(Android.Manifest.Permission.VIBRATE)]
[assembly: UsesPermission(Android.Manifest.Permission.WAKE_LOCK)]
[assembly: UsesPermission(Android.Manifest.Permission.WRITE_EXTERNAL_STORAGE)]
