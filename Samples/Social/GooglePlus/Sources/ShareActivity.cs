using System;

using Dot42.Manifest;

using Com.Google.Android.Gms.Common;
using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Plus.Model.People;

using Android.View;
using Android.Support.V4.App;
using Android.Widget;
using Android.Os;
using Android.Content;
using Android.Util;

namespace GooglePlusClient
{
   // Example of sharing with Google+ through the ACTION_SEND intent.
   [Activity(VisibleInLauncher = false)]
   public class ShareActivity : FragmentActivity, View.IOnClickListener, PlusClientFragment.OnSignedInListener
   {
      protected String TAG = typeof(ShareActivity).FullName;

      private const String STATE_SHARING = "resolving_error";

      private const String TAG_ERROR_DIALOG_FRAGMENT = "errorDialog";

      private const int REQUEST_CODE_PLUS_CLIENT_FRAGMENT = 1;
      private const int REQUEST_CODE_RESOLVE_GOOGLE_PLUS_ERROR = 2;
      private const int REQUEST_CODE_INTERACTIVE_POST = 3;

      // The button should say "View item" in English.
      private const String LABEL_VIEW_ITEM = "VIEW_ITEM";

      private EditText mEditSendText;
      private Button mSendButton;
      private PlusClientFragment mPlusClientFragment;
      private bool mSharing;

      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetContentView(R.Layouts.share_activity);
         mSendButton = (Button)FindViewById(R.Ids.send_interactive_button);
         mSendButton.SetOnClickListener(this);
         mSendButton.SetEnabled(true);
         mEditSendText = (EditText)FindViewById(R.Ids.share_prefill_edit);
         mPlusClientFragment =
                 PlusClientFragment.GetPlusClientFragment(this, MomentUtil.VISIBLE_ACTIVITIES);
         mSharing =
                 savedInstanceState != null && savedInstanceState.GetBoolean(STATE_SHARING, false);
      }

      override protected void OnSaveInstanceState(Bundle outState)
      {
         base.OnSaveInstanceState(outState);
         outState.PutBoolean(STATE_SHARING, mSharing);
      }

      public void OnClick(View view)
      {
         switch (view.GetId())
         {
            case R.Ids.send_interactive_button:
               // Set sharing so that the share is started in onSignedIn.
               mSharing = true;
               mPlusClientFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
               break;
         }
      }

      public void OnSignedIn(PlusClient plusClient)
      {
         if (!mSharing)
         {
            // The share button hasn't been clicked yet.
            return;
         }

         // Reset sharing so future calls to onSignedIn don't start a share.
         mSharing = false;
         int errorCode = GooglePlusUtil.CheckGooglePlusApp(this);
         if (errorCode == GooglePlusUtil.SUCCESS)
         {
            StartActivityForResult(getInteractivePostIntent(plusClient),
                    REQUEST_CODE_INTERACTIVE_POST);
         }
         else
         {
            // Prompt the user to install the Google+ app.
            GooglePlusErrorDialogFragment
                    .Create(errorCode, REQUEST_CODE_RESOLVE_GOOGLE_PLUS_ERROR)
                    .Show(GetSupportFragmentManager(), TAG_ERROR_DIALOG_FRAGMENT);
         }
      }

      override protected void OnActivityResult(int requestCode, int resultCode, Intent intent)
      {
         if (mPlusClientFragment.HandleOnActivityResult(requestCode, resultCode, intent))
         {
            return;
         }

         switch (requestCode)
         {
            case REQUEST_CODE_INTERACTIVE_POST:
               if (resultCode != RESULT_OK)
               {
                  Log.E(TAG, "Failed to create interactive post");
               }
               break;
            case REQUEST_CODE_RESOLVE_GOOGLE_PLUS_ERROR:
               if (resultCode != RESULT_OK)
               {
                  Log.E(TAG, "Unable to recover from missing Google+ app.");
               }
               else
               {
                  mPlusClientFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
               }
               break;
         }
      }

      private Intent getInteractivePostIntent(PlusClient plusClient)
      {
         // Create an interactive post with the "VIEW_ITEM" label. This will
         // create an enhanced share dialog when the post is shared on Google+.
         // When the user clicks on the deep link, ParseDeepLinkActivity will
         // immediately parse the deep link, and route to the appropriate resource.
         String action = "/?view=true";
         Android.Net.Uri callToActionUrl = Android.Net.Uri.Parse(GetString(R.Strings.plus_example_deep_link_url) + action);
         String callToActionDeepLinkId = GetString(R.Strings.plus_example_deep_link_id) + action;

         // Create an interactive post builder.
         PlusShare.Builder builder = new PlusShare.Builder(this, plusClient);

         // Set call-to-action metadata.
         builder.AddCallToAction(LABEL_VIEW_ITEM, callToActionUrl, callToActionDeepLinkId);

         // Set the target url (for desktop use).
         builder.SetContentUrl(Android.Net.Uri.Parse(GetString(R.Strings.plus_example_deep_link_url)));

         // Set the target deep-link ID (for mobile use).
         builder.SetContentDeepLinkId(GetString(R.Strings.plus_example_deep_link_id),
                 null, null, null);

         // Set the pre-filled message.
         builder.SetText(mEditSendText.GetText().ToString());

         return builder.GetIntent();
      }
   }

}
