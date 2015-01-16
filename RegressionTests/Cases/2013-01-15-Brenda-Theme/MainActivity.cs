using System;
using System.Linq;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using System.Collections.Generic;

//[assembly: Application("Test1", Theme = "@android:style/Theme.Holo")]  // Oke

[assembly: Application("Test1", Theme = "@style/MyTheme")] // Not oke?

namespace Test1
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
