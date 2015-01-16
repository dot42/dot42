using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

namespace ServiceTest
{
    [Activity(Label = "Service test activity", VisibleInLauncher = true)]
    public class TestActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var button = new Button(this) {Text = "Start"};
            SetContentView(button);
            button.Click += (s, x) => {
                var intent = new Intent(this, typeof (TestService));
                StartService(intent);
            };
        }
    }
}
