using System;

using Java.Util;

using Dot42;
using Dot42.Manifest;

using Com.Google.Android.Gms.Common;
using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Plus.Model;
using Com.Google.Android.Gms.Plus.Model.Moments;
using Com.Google.Android.Gms.Plus.Model.People;

using Android.View;
using Android.Support.V4.App;
using Android.Widget;
using Android.Os;
using Android.Content;
using Android.Util;

namespace GooglePlusClient
{
   // Example of writing moments through the PlusClient.
   [Activity(VisibleInLauncher = false)]
   public class MomentActivity : FragmentActivity,
      AdapterView<IListAdapter>.IOnItemClickListener, PlusClientFragment.OnSignedInListener
   {

      public const int REQUEST_CODE_PLUS_CLIENT_FRAGMENT = 0;

      private IListAdapter mListAdapter;
      private ListView mMomentListView;
      private ArrayList<IMoment> mPendingMoments;
      private PlusClientFragment mPlusClientFragment;

      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetContentView(R.Layouts.multi_moment_activity);

         mPlusClientFragment =
                 PlusClientFragment.GetPlusClientFragment(this, MomentUtil.VISIBLE_ACTIVITIES);
         mListAdapter = new ArrayAdapter<String>(
                 this, Android.R.Layout.Simple_list_item_1, MomentUtil.MOMENT_LIST);
         mMomentListView = (ListView)FindViewById(R.Ids.moment_list);
         mMomentListView.SetOnItemClickListener(this);
         mMomentListView.SetAdapter(mListAdapter);
         mPendingMoments = new ArrayList<IMoment>();
      }

      public void OnSignedIn(PlusClient plusClient) {
        mMomentListView.SetAdapter(mListAdapter);
        if (!mPendingMoments.IsEmpty()) {
            // Write all moments that were written while the client was disconnected.
            foreach (IMoment pendingMoment in mPendingMoments) {
                plusClient.WriteMoment(pendingMoment);
                Toast.MakeText(this, GetString(R.Strings.plus_write_moment_status),
                        Toast.LENGTH_SHORT).Show();
            }
        }
        mPendingMoments.Clear();
    }

      public void OnItemClick(AdapterView<object> adapterView, View view, int i, long l)
      {
         TextView textView = (TextView)view;
         String momentType = (String)textView.GetText();
         String targetUrl = MomentUtil.MOMENT_TYPES.Get(momentType);

         IItemScope target = new IItemScope_Builder()
             .SetUrl(targetUrl)
             .Build();

         IMoment_Builder momentBuilder = new IMoment_Builder();
         momentBuilder.SetType("http://schemas.google.com/" + momentType);
         momentBuilder.SetTarget(target);

         IItemScope result = MomentUtil.getResultFor(momentType);
         if (result != null)
         {
            momentBuilder.SetResult(result);
         }

         // Resolve the connection status, and write the moment once PlusClient is connected.
         mPendingMoments.Add(momentBuilder.Build());
         mPlusClientFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
      }

      override protected void OnActivityResult(int requestCode, int resultCode, Intent data)
      {
         mPlusClientFragment.HandleOnActivityResult(requestCode, resultCode, data);
      }
   }
}