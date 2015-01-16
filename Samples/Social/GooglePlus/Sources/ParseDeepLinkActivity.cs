using System;

using Java.Util;


using Dot42;
using Dot42.Manifest;

using Com.Google.Android.Gms.Common;
using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Plus.Model;
using Com.Google.Android.Gms.Plus.Model.Moments;
using Com.Google.Android.Gms.Plus.Model.People;

using Android.Text;
using Android.View;
using Android.Support.V4.App;
using Android.Widget;
using Android.Os;
using Android.Content;
using Android.Util;

namespace GooglePlusClient
{
   class ParseDeepLinkActivity : Android.App.Activity
   {
      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);

         String deepLinkId = PlusShare.GetDeepLinkId(this.GetIntent());
         Intent target = processDeepLinkId(deepLinkId);
         if (target != null)
         {
            StartActivity(target);
         }

         Finish();
      }

      // Get the intent for an activity corresponding to the deep link ID.
      private Intent processDeepLinkId(String deepLinkId)
      {
         Intent route;

         Android.Net.Uri uri = Android.Net.Uri.Parse(deepLinkId);
         if (uri.GetPath().StartsWith(GetString(R.Strings.plus_example_deep_link_id)))
         {
            route = new Intent().SetClass(GetApplicationContext(), typeof(SignInActivity));

            // Check for the call-to-action query parameter, and perform an action.
            String viewAction = uri.GetQueryParameter("view");
            if (!TextUtils.IsEmpty(viewAction) && "true".Equals(viewAction))
            {
               Toast.MakeText(this, "Performed a view", Toast.LENGTH_LONG).Show();
            }

         }
         else
         {
            route = new Intent().SetClass(GetApplicationContext(), typeof(PlusSampleActivity));
         }

         return route;
      }
   }
}
