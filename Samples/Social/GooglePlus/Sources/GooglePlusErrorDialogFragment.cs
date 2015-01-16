using System;

using Android.Os;
using Android.Support.V4.App;

using Com.Google.Android.Gms.Plus;

namespace GooglePlusClient
{
   class GooglePlusErrorDialogFragment : DialogFragment
   {
      public const String ARG_ERROR_CODE = "errorCode";
      public const String ARG_REQUEST_CODE = "requestCode";


      public GooglePlusErrorDialogFragment() { }

      override public Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
      {
         Bundle args = GetArguments();
         return GooglePlusUtil.GetErrorDialog(args.GetInt(ARG_ERROR_CODE), GetActivity(),
                 args.GetInt(ARG_REQUEST_CODE));
      }

      public static DialogFragment Create(int errorCode, int requestCode)
      {
         DialogFragment fragment = new GooglePlusErrorDialogFragment();
         Bundle args = new Bundle();
         args.PutInt(GooglePlusErrorDialogFragment.ARG_ERROR_CODE, errorCode);
         args.PutInt(GooglePlusErrorDialogFragment.ARG_REQUEST_CODE, requestCode);
         fragment.SetArguments(args);
         return fragment;
      }
   }
}
