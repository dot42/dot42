using System;

using Java.Util;
using Java.Lang;

using Android.Support.V4.App;
using Android.Os;
using Android.Content;

using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Common;

namespace GooglePlusClient
{
   class PlusClientFragment : Fragment,
      IGooglePlayServicesClient_IConnectionCallbacks,
      IGooglePlayServicesClient_IOnConnectionFailedListener,
      PlusClient.IOnAccessRevokedListener
   {
      // Tag to refer to this fragment.
      private const String TAG_PLUS_CLIENT = "plusClientFragment";

      // Tag to refer to an error (resolution) dialog.
      private const String TAG_ERROR_DIALOG = "plusClientFragmentErrorDialog";

      // Tag to refer to a progress dialog when sign in is requested.
      private const String TAG_PROGRESS_DIALOG = "plusClientFragmentProgressDialog";

      // Array of strings representing visible activities to request for {@link #getArguments()}.
      private const String ARG_VISIBLE_ACTIVITIES = "visible_activities";

      // Integer request code to apply to requests, as set by {@link #signIn(int)}.
      private const String STATE_REQUEST_CODE = "request_code";

      // Signed in successfully connection state.
      private static ConnectionResult CONNECTION_RESULT_SUCCESS =
              new ConnectionResult(ConnectionResult.SUCCESS, null);

      // An invalid request code to use to indicate that {@link #signIn(int)} hasn't been called.
      private const int INVALID_REQUEST_CODE = -1;

      // The PlusClient to connect.
      private PlusClient mPlusClient;

      // The last result from onConnectionFailed.
      private ConnectionResult mLastConnectionResult;

      // The request specified in signIn or INVALID_REQUEST_CODE if not signing in.
      private int mRequestCode;

      // A handler to post callbacks (rather than call them in a potentially reentrant way.)
      private Handler mHandler;

      // Local handler to send callbacks on sign in.
      private sealed class PlusClientFragmentHandler : Handler
      {
         private PlusClientFragment mFragment;

         public const int WHAT_SIGNED_IN = 1;

         public PlusClientFragmentHandler(PlusClientFragment fragment)
            : base(Looper.GetMainLooper())
         {
            mFragment = fragment;
         }

         override public void HandleMessage(Message msg)
         {
            if (msg.What != WHAT_SIGNED_IN)
            {
               return;
            }

            Android.App.Activity activity = mFragment.GetActivity();
            if (mFragment.mPlusClient.IsConnected() && activity is OnSignedInListener)
            {
               ((OnSignedInListener)activity).OnSignedIn(mFragment.mPlusClient);
            }
         }
      }

      // Listener interface for sign in events. Activities hosting a PlusClientFragment
      // must implement this.
      public interface OnSignedInListener
      {
         // Called when the PlusClient has been connected successfully
         void OnSignedIn(PlusClient plusClient);
      }


      // Attach a PlusClient managing fragment to you activity.
      public static PlusClientFragment GetPlusClientFragment(
              FragmentActivity activity, String[] visibleActivities)
      {
         if (!(activity is OnSignedInListener))
         {
            throw new ArgumentException(
                    "The activity must implement OnSignedInListener to receive callbacks.");
         }

         // Check if the fragment is already attached.
         FragmentManager fragmentManager = activity.GetSupportFragmentManager();
         Fragment fragment = fragmentManager.FindFragmentByTag(TAG_PLUS_CLIENT);
         if (fragment is PlusClientFragment)
         {
            // The fragment is attached.  If it has the right visible activities, return it.
            if (Arrays.Equals(visibleActivities,
                    fragment.GetArguments().GetStringArray(ARG_VISIBLE_ACTIVITIES)))
            {
               return (PlusClientFragment)fragment;
            }
         }

         FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();
         // If a fragment was already attached, remove it to clean up.
         if (fragment != null)
         {
            fragmentTransaction.Remove(fragment);
         }

         // Create a new fragment and attach it to the fragment manager.
         Bundle arguments = new Bundle();
         arguments.PutStringArray(ARG_VISIBLE_ACTIVITIES, visibleActivities);
         PlusClientFragment signInFragment = new PlusClientFragment();
         signInFragment.SetArguments(arguments);
         fragmentTransaction.Add(signInFragment, TAG_PLUS_CLIENT);
         fragmentTransaction.Commit();
         return signInFragment;
      }

      public override void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);

         // Retain instance to avoid reconnecting on rotate.  This means that onDestroy and onCreate
         // will not be called on configuration changes.
         SetRetainInstance(true);
         mHandler = new PlusClientFragmentHandler(this);

         // Create the PlusClient.
         PlusClient.Builder plusClientBuilder =
                 new PlusClient.Builder(GetActivity().GetApplicationContext(), this, this);
         String[] visibleActivities = GetArguments().GetStringArray(ARG_VISIBLE_ACTIVITIES);
         if (visibleActivities != null && visibleActivities.Length > 0)
         {
            plusClientBuilder.SetVisibleActivities(visibleActivities);
         }
         mPlusClient = plusClientBuilder.Build();

         if (savedInstanceState == null)
         {
            mRequestCode = INVALID_REQUEST_CODE;
         }
         else
         {
            mRequestCode = savedInstanceState.GetInt(STATE_REQUEST_CODE, INVALID_REQUEST_CODE);
         }
      }

      // Disconnects the PlusClient to avoid leaks.
      override public void OnDestroy()
      {
         base.OnDestroy();
         if (mPlusClient.IsConnecting() || mPlusClient.IsConnected())
         {
            mPlusClient.Disconnect();
         }
      }

      override public void OnSaveInstanceState(Bundle outState)
      {
         base.OnSaveInstanceState(outState);
         outState.PutInt(STATE_REQUEST_CODE, mRequestCode);
      }

      override public void OnResume()
      {
         base.OnResume();
         if (mRequestCode == INVALID_REQUEST_CODE)
         {
            // No user interaction, hide the progress dialog.
            hideProgressDialog();
            hideErrorDialog();
         }
         else if (mLastConnectionResult != null && !mLastConnectionResult.IsSuccess()
               && !isShowingErrorDialog())
         {
            showProgressDialog();
         }
      }

      override public void OnStart()
      {
         base.OnStart();
         if (mRequestCode == INVALID_REQUEST_CODE)
         {
            mLastConnectionResult = null;
            connectPlusClient();
         }
      }

      public void OnConnected(Bundle connectionHint)
      {
         // Successful connection!
         mLastConnectionResult = CONNECTION_RESULT_SUCCESS;
         mRequestCode = INVALID_REQUEST_CODE;

         if (IsResumed())
         {
            hideProgressDialog();
         }

         Android.App.Activity activity = GetActivity();
         if (activity is OnSignedInListener)
         {
            ((OnSignedInListener)activity).OnSignedIn(mPlusClient);
         }
      }

      public void OnConnectionFailed(ConnectionResult connectionResult)
      {
         mLastConnectionResult = connectionResult;
         // On a failed connection try again.
         if (IsResumed() && mRequestCode != INVALID_REQUEST_CODE)
         {
            resolveLastResult();
         }
      }

      public void OnAccessRevoked(ConnectionResult status)
      {
         // Reconnect to get a new mPlusClient.
         mLastConnectionResult = null;
         // Cancel sign in.
         mRequestCode = INVALID_REQUEST_CODE;

         // Reconnect to fetch the sign-in (account chooser) intent from the plus client.
         connectPlusClient();
      }

      override public void OnActivityCreated(Bundle savedInstanceState)
      {
         base.OnActivityCreated(savedInstanceState);
         // Let new activities know the signed-in state.
         if (mPlusClient.IsConnected())
         {
            mHandler.SendEmptyMessage(PlusClientFragmentHandler.WHAT_SIGNED_IN);
         }
      }

      public void OnDisconnected()
      {
         // Do nothing.
      }

      // Shows any UI required to resolve the error connecting.
      public void SignIn(int requestCode)
      {
         if (requestCode < 0)
         {
            throw new ArgumentException("A non-negative request code is required.");
         }

         if (mPlusClient.IsConnected())
         {
            // Already connected!  Schedule callback.
            mHandler.SendEmptyMessage(PlusClientFragmentHandler.WHAT_SIGNED_IN);
            return;
         }

         if (mRequestCode != INVALID_REQUEST_CODE)
         {
            // We're already signing in.
            return;
         }

         mRequestCode = requestCode;
         if (mLastConnectionResult == null)
         {
            // We're starting up, show progress.
            showProgressDialog();
            return;
         }

         resolveLastResult();
      }

      // Perform resolution given a non-null result.
      private void resolveLastResult()
      {
         if (GooglePlayServicesUtil.IsUserRecoverableError(mLastConnectionResult.GetErrorCode()))
         {
            // Show a dialog to install or enable Google Play services.
            showErrorDialog(ErrorDialogFragment.Create(mLastConnectionResult.GetErrorCode(),
                    mRequestCode));
            return;
         }

         if (mLastConnectionResult.HasResolution())
         {
            startResolution();
         }
      }

      public sealed class ErrorDialogFragment : GooglePlayServicesErrorDialogFragment
      {
         public static ErrorDialogFragment Create(int errorCode, int requestCode)
         {
            ErrorDialogFragment fragment = new ErrorDialogFragment();
            fragment.SetArguments(CreateArguments(errorCode, requestCode));
            return fragment;
         }

         override public void OnCancel(IDialogInterface dialog)
         {
            base.OnCancel(dialog);
            FragmentActivity activity = GetActivity();
            if (activity == null)
            {
               return;
            }

            Fragment fragment =
                    activity.GetSupportFragmentManager().FindFragmentByTag(TAG_PLUS_CLIENT);
            if (fragment is PlusClientFragment)
            {
               ((PlusClientFragment)fragment).onDialogCanceled(GetTag());
            }
         }

         override public void OnDismiss(IDialogInterface dialog)
         {
            base.OnDismiss(dialog);
            FragmentActivity activity = GetActivity();
            if (activity == null)
            {
               return;
            }

            Fragment fragment =
                    activity.GetSupportFragmentManager().FindFragmentByTag(TAG_PLUS_CLIENT);
            if (fragment is PlusClientFragment)
            {
               ((PlusClientFragment)fragment).onDialogDismissed(GetTag());
            }
         }
      }

      private void onDialogCanceled(String tag)
      {
         mRequestCode = INVALID_REQUEST_CODE;
         hideProgressDialog();
      }

      private void onDialogDismissed(String tag)
      {
         if (TAG_PROGRESS_DIALOG.Equals(tag))
         {
            mRequestCode = INVALID_REQUEST_CODE;
            hideProgressDialog();
         }
      }

      private void showProgressDialog()
      {
         DialogFragment progressDialog =
                 (DialogFragment)GetFragmentManager().FindFragmentByTag(TAG_PROGRESS_DIALOG);
         if (progressDialog == null)
         {
            progressDialog = ProgressDialogFragment.create();
            progressDialog.Show(GetFragmentManager(), TAG_PROGRESS_DIALOG);
         }
      }

      public sealed class ProgressDialogFragment : DialogFragment
      {

         private const String ARG_MESSAGE = "message";

         public static ProgressDialogFragment create(int message)
         {
            ProgressDialogFragment progressDialogFragment = new ProgressDialogFragment();
            Bundle args = new Bundle();
            args.PutInt(ARG_MESSAGE, message);
            progressDialogFragment.SetArguments(args);
            return progressDialogFragment;
         }

         public static ProgressDialogFragment create()
         {
            return create(R.Strings.progress_message);
         }

         override public Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
         {
            Android.App.ProgressDialog progressDialog = new Android.App.ProgressDialog(GetActivity());
            progressDialog.SetIndeterminate(true);
            progressDialog.SetMessage(GetString(GetArguments().GetInt(ARG_MESSAGE)));
            return progressDialog;
         }

         override public void OnCancel(IDialogInterface dialog)
         {
            base.OnCancel(dialog);
            FragmentActivity activity = GetActivity();
            if (activity == null)
            {
               return;
            }

            Fragment fragment = activity.GetSupportFragmentManager().FindFragmentByTag(TAG_PLUS_CLIENT);
            if (fragment is PlusClientFragment)
            {
               ((PlusClientFragment)fragment).onDialogCanceled(GetTag());
            }
         }

         override public void OnDismiss(IDialogInterface dialog)
         {
            base.OnDismiss(dialog);
            FragmentActivity activity = GetActivity();
            if (activity == null)
            {
               return;
            }

            Fragment fragment =
                     activity.GetSupportFragmentManager().FindFragmentByTag(TAG_PLUS_CLIENT);
            if (fragment is PlusClientFragment)
            {
               ((PlusClientFragment)fragment).onDialogDismissed(GetTag());
            }
         }
      }


      protected void hideProgressDialog()
      {
         FragmentManager manager = GetFragmentManager();
         if (manager != null)
         {
            DialogFragment progressDialog = (DialogFragment)manager
                    .FindFragmentByTag(TAG_PROGRESS_DIALOG);
            if (progressDialog != null)
            {
               progressDialog.Dismiss();
            }
         }
      }

      private void showErrorDialog(DialogFragment errorDialog)
      {
         DialogFragment oldErrorDialog =
                 (DialogFragment)GetFragmentManager().FindFragmentByTag(TAG_ERROR_DIALOG);
         if (oldErrorDialog != null)
         {
            oldErrorDialog.Dismiss();
         }

         errorDialog.Show(GetFragmentManager(), TAG_ERROR_DIALOG);
      }

      private bool isShowingErrorDialog()
      {
         DialogFragment errorDialog =
                 (DialogFragment)GetFragmentManager().FindFragmentByTag(TAG_ERROR_DIALOG);
         return errorDialog != null && !errorDialog.IsHidden();
      }

      private void hideErrorDialog()
      {
         DialogFragment errorDialog =
                 (DialogFragment)GetFragmentManager().FindFragmentByTag(TAG_ERROR_DIALOG);
         if (errorDialog != null)
         {
            errorDialog.Dismiss();
         }
      }

      private void startResolution()
      {
         try
         {
            mLastConnectionResult.StartResolutionForResult(GetActivity(), mRequestCode);
            hideProgressDialog();
         }
         catch (IntentSender.SendIntentException e)
         {
            // The intent we had is not valid right now, perhaps the remote process died.
            // Try to reconnect to get a new resolution intent.
            mLastConnectionResult = null;
            showProgressDialog();
            connectPlusClient();
         }
      }

      public bool HandleOnActivityResult(int requestCode, int resultCode, Intent data)
      {
         if (requestCode != mRequestCode)
         {
            return false;
         }

         switch (resultCode)
         {
            case Android.App.Activity.RESULT_OK:
               mLastConnectionResult = null;
               connectPlusClient();
               break;
            case Android.App.Activity.RESULT_CANCELED:
               // User canceled sign in, clear the request code.
               mRequestCode = INVALID_REQUEST_CODE;

               // Attempt to connect again.
               connectPlusClient();
               break;
         }
         return true;
      }

      // Sign out of the app.
      public void SignOut()
      {
         if (mPlusClient.IsConnected())
         {
            mPlusClient.ClearDefaultAccount();
         }

         if (mPlusClient.IsConnecting() || mPlusClient.IsConnected())
         {
            mPlusClient.Disconnect();
            // Reconnect to get a new mPlusClient.
            mLastConnectionResult = null;
            // Cancel sign in.
            mRequestCode = INVALID_REQUEST_CODE;

            // Reconnect to fetch the sign-in (account chooser) intent from the plus client.
            connectPlusClient();
         }
      }

      // Revoke access to the current app.
      public void RevokeAccessAndDisconnect()
      {
         if (mPlusClient.IsConnected())
         {
            mPlusClient.RevokeAccessAndDisconnect(this);
         }
      }


      // Attempts to connect the client to Google Play services if the client isn't already connected,
      // and isn't in the process of being connected.
      private void connectPlusClient()
      {
         if (!mPlusClient.IsConnecting() && !mPlusClient.IsConnected())
         {
            mPlusClient.Connect();
         }
      }

   }


}
