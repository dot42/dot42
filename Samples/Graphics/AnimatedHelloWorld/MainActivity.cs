using System;
using Android.App;
using Android.Os;
using Android.View.Animation;
using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Animated Hello World")]

namespace AnimatedHelloWorld
{
    [Activity(Icon = "Icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);
            var text = FindViewById(R.Ids.theLabel);
            var animation = AnimationUtils.LoadAnimation(this, R.Anims.Animation);
            text.SetAnimation(animation);
            animation.Start();
        }
    }
}
