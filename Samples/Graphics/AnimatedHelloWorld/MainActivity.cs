using System;
using Android.App;
using Android.OS;
using Android.Views.Animations;
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
            SetContentView(R.Layout.MainLayout);
            var text = FindViewById(R.Id.theLabel);
            var animation = AnimationUtils.LoadAnimation(this, R.Anim.Animation);
            text.Animation = (animation);
            animation.Start();
        }
    }
}
