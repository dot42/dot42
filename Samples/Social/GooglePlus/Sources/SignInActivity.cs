using System;

using Dot42;
using Dot42.Manifest;

using Com.Google.Android.Gms.Common;
using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Plus.Model.People;
using Android.Views;
using Android.Support.V4.App;
using Android.Widget;using Android.OS;
using Android.Content;

namespace GooglePlusClient
{
   [Activity(VisibleInLauncher = false)]
   public class SignInActivity : FragmentActivity, View.IOnClickListener, PlusClientFragment.OnSignedInListener
   {
      public const int REQUEST_CODE_PLUS_CLIENT_FRAGMENT = 0;

      private TextView mSignInStatus;
      private PlusClientFragment mSignInFragment;

      protected override void OnCreate(Bundle savedInstance)
      {
         base.OnCreate(savedInstance);

         SetContentView(R.Layout.sign_in_activity);

         mSignInFragment = PlusClientFragment.GetPlusClientFragment(this, MomentUtil.VISIBLE_ACTIVITIES);

         FindViewById(R.Id.sign_in_button).SetOnClickListener(this);
         FindViewById(R.Id.sign_out_button).SetOnClickListener(this);
         FindViewById(R.Id.revoke_access_button).SetOnClickListener(this);
         mSignInStatus = (TextView)FindViewById(R.Id.sign_in_status);
      }

      public void OnClick(View view)
      {
         switch (view.GetId())
         {
            case R.Id.sign_out_button:
               resetAccountState();
               mSignInFragment.SignOut();
               break;
            case R.Id.sign_in_button:
               mSignInFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
               break;
            case R.Id.revoke_access_button:
               resetAccountState();
               mSignInFragment.RevokeAccessAndDisconnect();
               break;
         }
      }

      override protected void OnActivityResult(int requestCode, int responseCode, Intent intent)
      {
         mSignInFragment.HandleOnActivityResult(requestCode, responseCode, intent);
      }

      public void OnSignedIn(PlusClient plusClient)
      {
         mSignInStatus.Text = (GetString(R.String.signed_in_status));

         // We can now obtain the signed-in user's profile information.
         IPerson currentPerson = plusClient.GetCurrentPerson();
         if (currentPerson != null)
         {
            String greeting = GetString(R.String.greeting_status, currentPerson.GetDisplayName());
            mSignInStatus.Text = (greeting);
         }
         else
         {
            resetAccountState();
         }
      }

      private void resetAccountState()
      {
         mSignInStatus.Text = (GetString(R.String.signed_out_status));
      }
   }
}
