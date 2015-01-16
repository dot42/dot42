using System;

using Android.App;
using Android.Os;
using Android.Widget;
using Android.Content;
using Android.View;

using Dot42;
using Dot42.Manifest;

using Com.Google.Android.Gms.Common;
using Com.Google.Android.Gms.Plus;

[assembly: Application("GooglePlusClient")]

namespace GooglePlusClient
{
   [Activity]
   public class MainActivity : Activity, View.IOnClickListener
   {
      protected override void OnCreate(Bundle savedInstance)
      {
         base.OnCreate(savedInstance);
         SetContentView(R.Layouts.MainLayout);
         FindViewById(R.Ids.sign_in_button).SetOnClickListener(this);
      }

      public void OnClick(View view)
      {
         if (view.GetId() == R.Ids.sign_in_button)
         {
         }
      }
   }
}
