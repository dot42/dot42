using System;

using Com.Google.Android.Gms.Common;

using Android.Support.V4.App;
using Android.Os;

namespace GooglePlusClient
{
   class GooglePlayServicesErrorDialogFragment : DialogFragment
   {
      public const String ARG_ERROR_CODE = "errorCode";
      public const String ARG_REQUEST_CODE = "requestCode";

      public override Android.App.Dialog OnCreateDialog(Bundle bundle)
      {
         Bundle args = GetArguments();
         return GooglePlayServicesUtil.GetErrorDialog(args.GetInt(ARG_ERROR_CODE), GetActivity(),
            args.GetInt(ARG_REQUEST_CODE));
      }

      public static Bundle CreateArguments(int errorCode, int requestCode)
      {
         Bundle args = new Bundle();
         args.PutInt(GooglePlayServicesErrorDialogFragment.ARG_ERROR_CODE, errorCode);
         args.PutInt(GooglePlayServicesErrorDialogFragment.ARG_REQUEST_CODE, requestCode);
         return args;
      }
   }
}
