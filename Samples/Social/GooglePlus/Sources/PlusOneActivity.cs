using System;

using Dot42.Manifest;

using Android.App;
using Android.Os;

using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Common;

namespace GooglePlusClient
{
   // Example usage of the +1 button.
   [Activity(VisibleInLauncher = false)]
   public class PlusOneActivity :
      Activity,
      IGooglePlayServicesClient_IConnectionCallbacks,
      IGooglePlayServicesClient_IOnConnectionFailedListener
   {
      private const String URL = "https://developers.google.com/+";

      // The request code must be 0 or higher.
      private const int PLUS_ONE_REQUEST_CODE = 0;

      private PlusClient mPlusClient;
      private PlusOneButton mPlusOneSmallButton;
      private PlusOneButton mPlusOneMediumButton;
      private PlusOneButton mPlusOneTallButton;
      private PlusOneButton mPlusOneStandardButton;
      private PlusOneButton mPlusOneStandardButtonWithAnnotation;

      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetContentView(R.Layouts.plus_one_activity);

         // The +1 button does not require scopes.
         mPlusClient = new PlusClient.Builder(this, this, this)
          .ClearScopes()
          .Build();

         // The PlusOneButton can be configured in code, but in this example we
         // have set the parameters in the layout.
         // Example:
         // mPlusOneSmallButton.setAnnotation(PlusOneButton.ANNOTATION_INLINE);
         // mPlusOneSmallButton.setSize(PlusOneButton.SIZE_MEDIUM);
         mPlusOneSmallButton = (PlusOneButton)FindViewById(R.Ids.plus_one_small_button);
         mPlusOneMediumButton = (PlusOneButton)FindViewById(R.Ids.plus_one_medium_button);
         mPlusOneTallButton = (PlusOneButton)FindViewById(R.Ids.plus_one_tall_button);
         mPlusOneStandardButton = (PlusOneButton)FindViewById(R.Ids.plus_one_standard_button);
         mPlusOneStandardButtonWithAnnotation = (PlusOneButton)FindViewById(R.Ids.plus_one_standard_ann_button);
      }

      override protected void OnStart()
      {
         base.OnStart();
         mPlusClient.Connect();
      }

      override protected void OnResume()
      {
         base.OnResume();
         // Refresh the state of the +1 button each time we receive focus.
         mPlusOneSmallButton.Initialize(mPlusClient, URL, PLUS_ONE_REQUEST_CODE);
         mPlusOneMediumButton.Initialize(mPlusClient, URL, PLUS_ONE_REQUEST_CODE);
         mPlusOneTallButton.Initialize(mPlusClient, URL, PLUS_ONE_REQUEST_CODE);
         mPlusOneStandardButton.Initialize(mPlusClient, URL, PLUS_ONE_REQUEST_CODE);
         mPlusOneStandardButtonWithAnnotation.Initialize(mPlusClient, URL, PLUS_ONE_REQUEST_CODE);
      }

      override protected void OnStop()
      {
         base.OnStop();
         mPlusClient.Disconnect();
      }

      public void OnConnectionFailed(ConnectionResult status)
      {
         // Nothing to do.
      }

      public void OnConnected(Bundle connectionHint)
      {
         // Nothing to do.
      }


      public void OnDisconnected()
      {
         // Nothing to do.
      }
   }
}
