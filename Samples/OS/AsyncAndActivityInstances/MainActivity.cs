using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Java.Lang;

[assembly: Application("AsyncAndActivityInstances")]

namespace TestAppDot42Async
{
    [Activity]
    public class MainActivity : Activity
    {
        private TextView status;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            status = FindViewById<TextView>(R.Ids.txtStatus);

            var mySynchronizedButton = FindViewById<Button>(R.Ids.mySynchronizedButton);
            mySynchronizedButton.Click += mySynchronizedButton_Click;

            var myNotSynchronizedButton = FindViewById<Button>(R.Ids.myNotSynchronizedButton);
            myNotSynchronizedButton.Click += myNotSynchronizedButton_Click;

            //Set the static synchronization context of the current/latest 'this', allowing the code after the awit to 
            //resume on the 'current' this.
            SynchronizationContext.SetSynchronizationContext(this);
        }

        private async void mySynchronizedButton_Click(object sender, EventArgs e)
        {
            status.Text = "Waiting (synchronized)...";
            //We specify that the new Task should use the synchoniczation context bound to the activity, so the code
            //after the await will runs on the updated this of the activity.
            await Task.Delay(3000).ConfigureAwait(this);
            status.Text = "I'm back";
        }

        private async void myNotSynchronizedButton_Click(object sender, EventArgs e)
        {
            status.Text = "Waiting (not synchronized)...";
            //We do NOT specify to use a synchonization context to be bound to any activity, so the code 
            //after the await will runs on the now current this.
            await Task.Delay(3000);
            status.Text = "I'm back";
        }
    }
}
