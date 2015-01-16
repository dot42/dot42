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
   class ListPeopleActivity :
      FragmentActivity,
      PlusClientFragment.OnSignedInListener,
      PlusClient.IOnPeopleLoadedListener
   {
      private String TAG = typeof(ListPeopleActivity).Name;

      private const int REQUEST_CODE_PLUS_CLIENT_FRAGMENT = 0;

      private ArrayAdapter<String> mListAdapter;
      private ListView mPersonListView;
      private ArrayList<String> mListItems;
      private PlusClientFragment mPlusClientFragment;

      override protected void OnCreate(Bundle savedInstanceState)
      {
         base.OnCreate(savedInstanceState);
         SetContentView(R.Layouts.person_list_activity);

         mListItems = new ArrayList<String>();
         mListAdapter = new ArrayAdapter<String>(this, Android.R.Layout.Simple_list_item_1,
                 mListItems);
         mPersonListView = (ListView)FindViewById(R.Ids.person_list);
         mPersonListView.SetAdapter(mListAdapter);
         mPlusClientFragment = PlusClientFragment.GetPlusClientFragment(this,
                 MomentUtil.VISIBLE_ACTIVITIES);
      }

      override protected void OnResume()
      {
         base.OnResume();
         mPlusClientFragment.SignIn(REQUEST_CODE_PLUS_CLIENT_FRAGMENT);
      }

      // Called when PlusClient has been connected
      public void OnSignedIn(PlusClient plusClient)
      {
         plusClient.LoadPeople(this, IPerson_ICollectionConstants.VISIBLE,
                 IPerson_IOrderByConstants.ALPHABETICAL, 10, null);
      }

      public void OnPeopleLoaded(ConnectionResult status, PersonBuffer personBuffer,
              String nextPageToken)
      {
         if (status.GetErrorCode() == ConnectionResult.SUCCESS)
         {
            mListItems.Clear();
            try
            {
               int count = personBuffer.GetCount();
               for (int i = 0; i < count; i++)
               {
                  mListItems.Add(personBuffer.Get(i).GetDisplayName());
               }
            }
            finally
            {
               personBuffer.Close();
            }

            mListAdapter.NotifyDataSetChanged();
         }
         else
         {
            Log.E(TAG, "Error when listing people: " + status);
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
   }
}
