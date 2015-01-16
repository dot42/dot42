using Android.App;
using Android.Os;
using Dot42.Manifest;

[assembly: Application("Various manifest tests")]
[assembly: Package(InstallLocation = InstallLocations.PreferExternal)]

[assembly: UsesFeature("com.dot42.myFeature")]
[assembly: UsesFeature("com.dot42.myReqFeature", true)]
[assembly: UsesFeature("com.dot42.myOptionalFeature", false)]

[assembly: Permission("com.dot42.permissions.Some")]
[assembly: PermissionGroup("com.dot42.permissions.SomeGroup")]

[assembly: UsesPermission(Android.Manifest.Permission.ACCOUNT_MANAGER)]

[assembly: UsesLibrary("com.dot42.MyLibrary", true)]

[assembly: UsesOpenGL(5)]

[assembly: SupportsScreens(NormalScreens = true, XLargeScreens = true, SmallScreens = false)]

namespace Various
{
    [Activity]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
        }
    }
}
