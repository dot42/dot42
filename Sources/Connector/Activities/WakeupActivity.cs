using Android.App;
using Android.OS;
using Android.Views;
using Java.Lang;
using Dot42;
using Dot42.Manifest;

namespace Dot42.Connector.Activities
{
    [Activity(Icon = "Icon", Label = "Wakeup!", VisibleInLauncher = false, Exported = true, LaunchMode = LaunchModes.SingleInstance, FinishOnTaskLaunch=true, StateNotNeeded=true, NoHistory=false, ExcludeFromRecents=true)] 
    public class WakeupActivity : Activity, IRunnable
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
			
			var window = GetWindow();
			window.AddFlags(IWindowManager_LayoutParams.FLAG_SHOW_WHEN_LOCKED | IWindowManager_LayoutParams.FLAG_TURN_SCREEN_ON | IWindowManager_LayoutParams.FLAG_DISMISS_KEYGUARD);
        
            SetContentView(R.Layout.MainLayout);
        
			var handler = new Handler(); 
			handler.PostDelayed(this, 200);			
        }
		
		public void Run() 
		{
			Finish();
		}
    }
}
