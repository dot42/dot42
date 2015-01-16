using System;

using Java.Util;

using Dot42;
using Dot42.Manifest;

using Com.Google.Android.Gms.Common;
using Com.Google.Android.Gms.Plus;
using Com.Google.Android.Gms.Plus.Model.People;
using Com.Google.Android.Gms.Plus.Model.Moments;

using Android.View;
using Android.Support.V4.App;
using Android.Widget;
using Android.Os;
using Android.Content;
using Android.Util;

namespace GooglePlusClient
{
   [Activity(VisibleInLauncher = false)]
   class ListMomentsActivity : FragmentActivity,
      PlusClientFragment.OnSignedInListener,
      PlusClient.IOnMomentsLoadedListener,
      AdapterView<IListAdapter>.IOnItemClickListener
   {
      private String TAG = typeof(MomentActivity).Name;
      private const int REQUEST_CODE_PLUS_CLIENT_FRAGMENT = 0;

      private ListView mMomentListView;
      private MomentListAdapter mMomentListAdapter;
      private ArrayList<IMoment> mListItems;
      private ArrayList<IMoment> mPendingDeletion;

      private PlusClientFragment mPlusClientFragment;

      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetContentView(R.Layouts.list_moments_activity);

         mPendingDeletion = new ArrayList<IMoment>();
         mListItems = new ArrayList<IMoment>();
         mMomentListAdapter = new MomentListAdapter(this, Android.R.Layout.Simple_list_item_1,
                 mListItems);
         mMomentListView = (ListView)FindViewById(R.Ids.moment_list);
         mMomentListView.SetAdapter(mMomentListAdapter);
         mMomentListView.SetOnItemClickListener(this);
         mPlusClientFragment = PlusClientFragment.GetPlusClientFragment(this,
                 MomentUtil.VISIBLE_ACTIVITIES);
      }

      override protected void OnResume()
      {
         base.OnResume();
         mPlusClientFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
      }

      // Called when the {@link com.google.android.gms.plus.PlusClient} has been connected
      // successfully.
      public void OnSignedIn(PlusClient plusClient)
      {
         int deleteCount = mPendingDeletion.Size();
         for (int i = 0; i < deleteCount; i++)
         {
            plusClient.RemoveMoment(mPendingDeletion.Get(i).GetId());
         }

         mPendingDeletion.Clear();
         plusClient.LoadMoments(this);
      }

      public void OnMomentsLoaded(ConnectionResult status, MomentBuffer momentBuffer,
              String nextPageToken, String updated)
      {
         if (status.GetErrorCode() == ConnectionResult.SUCCESS)
         {
            mListItems.Clear();
            try
            {
               int count = momentBuffer.GetCount();
               for (int i = 0; i < count; i++)
               {
                  mListItems.Add(momentBuffer.Get(i).Freeze());
               }
            }
            finally
            {
               momentBuffer.Close();
            }

            mMomentListAdapter.NotifyDataSetChanged();
         }
         else
         {
            Log.E(TAG, "Error when loading moments: " + status.GetErrorCode());
         }
      }

      // Delete a moment when clicked.
      public void OnItemClick(AdapterView<object> parent, View view, int position, long id)
      {
         IMoment moment = mMomentListAdapter.GetItem(position);
         if (moment != null)
         {
            mPendingDeletion.Add(moment);
            Toast.MakeText(this, GetString(R.Strings.plus_remove_moment_status),
                    Toast.LENGTH_SHORT).Show();
            mPlusClientFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
         }
      }

      override protected void OnActivityResult(int requestCode, int resultCode, Intent data)
      {
         if (mPlusClientFragment.HandleOnActivityResult(requestCode, resultCode, data))
         {
            switch (resultCode)
            {
               case RESULT_CANCELED:
                  // User canceled sign in.
                  Toast.MakeText(this, R.Strings.greeting_status_sign_in_required,
                          Toast.LENGTH_LONG).Show();
                  Finish();
                  break;
            }
         }
      }

      // Array adapter that maintains a Moment list.
      private class MomentListAdapter : ArrayAdapter<IMoment>
      {
         private ArrayList<IMoment> items;
         private Context mContext; // reference to outer 

         public MomentListAdapter(Context context, int textViewResourceId,
                 ArrayList<IMoment> objects)
            : base(context, textViewResourceId, objects)
         {
            items = objects;
            mContext = context;
         }

         override public View GetView(int position, View convertView, ViewGroup parent)
         {
            View v = convertView;
            if (v == null)
            {
               LayoutInflater vi = (LayoutInflater) mContext.GetSystemService(
                       Context.LAYOUT_INFLATER_SERVICE);
               v = vi.Inflate(R.Layouts.moment_row, null);
            }
            IMoment moment = items.Get(position);
            if (moment != null)
            {
               TextView typeView = (TextView)v.FindViewById(R.Ids.moment_type);
               TextView titleView = (TextView)v.FindViewById(R.Ids.moment_title);

               String type = Android.Net.Uri.Parse(moment.GetType()).GetPath().Substring(1);
               typeView.SetText(type);

               if (moment.GetTarget() != null)
               {
                  titleView.SetText(moment.GetTarget().GetName());
               }
            }

            return v;
         }
      }
   }
}
