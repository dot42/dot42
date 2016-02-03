using System;

using Android.App;using Android.OS;
using Android.Widget;
using Android.Content;using Android.Views;

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
         SetContentView(R.Layout.MainLayout);
         FindViewById(R.Id.sign_in_button).SetOnClickListener(this);
      }

      public void OnClick(View view)
      {
         if (view.Id == R.Id.sign_in_button)
         {
         }
      }
   }
}
