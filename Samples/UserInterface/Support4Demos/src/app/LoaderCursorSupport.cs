/*
 * Copyright (C) 2010 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;

using Android.Database;
using Android.Net;
using Android.Os;
using Android.Provider;
using Android.Text;
using Android.Util;
using Android.View;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of the use of a CursorLoader to load and display contacts
     * data in a fragment.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/loader_cursor_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class LoaderCursorSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            FragmentManager fm = GetSupportFragmentManager();

            // Create the list fragment and Add it as our sole content.
            if (fm.FindFragmentById(global::Android.R.Id.Content) == null) {
                CursorLoaderListFragment list = new CursorLoaderListFragment();
                fm.BeginTransaction().Add(global::Android.R.Id.Content, list).Commit();
            }
        }


        public class CursorLoaderListFragment : ListFragment, LoaderManager.ILoaderCallbacks<ICursor> 
        {
            private class MyOnQueryTextListenerCompat : SearchViewCompat.OnQueryTextListenerCompat
            {
                readonly CursorLoaderListFragment cursorLoaderListFragment;

                public MyOnQueryTextListenerCompat(CursorLoaderListFragment cursorLoaderListFragment)
                {
                    this.cursorLoaderListFragment = cursorLoaderListFragment;
                }

                public override bool OnQueryTextChange(string newText) {
                    // Called when the action bar search text has changed.  Update
                    // the search filter, and restart the loader to do a new query
                    // with this filter.
                    string newFilter = !TextUtils.IsEmpty(newText) ? newText : null;
                    // Don't do anything if the filter hasn't actually changed.
                    // Prevents restarting the loader when restoring state.
                    if (cursorLoaderListFragment.mCurFilter == null && newFilter == null) {
                        return true;
                    }
                    if (cursorLoaderListFragment.mCurFilter != null && cursorLoaderListFragment.mCurFilter.Equals(newFilter)) {
                        return true;
                    }
                    cursorLoaderListFragment.mCurFilter = newFilter;
                    cursorLoaderListFragment.GetLoaderManager().RestartLoader(0, null, cursorLoaderListFragment);
                    return true;
                }
            }

            private class MyOnCloseListenerCompat : SearchViewCompat.OnCloseListenerCompat
            {
                readonly View mSearchView;

                public MyOnCloseListenerCompat(View searchView)
                {
                    mSearchView = searchView;
                }

                public override bool OnClose()
                {
                    if (!TextUtils.IsEmpty(SearchViewCompat.GetQuery(mSearchView)))
                    {
                        SearchViewCompat.SetQuery(mSearchView, null, true);
                    }
                    return true;
                }
            }


            // This is the Adapter being used to display the list's data.
            SimpleCursorAdapter mAdapter;

            // If non-null, this is the current filter the user has provided.
            string mCurFilter;

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);

                // Give some text to display if there is no data.  In a real
                // application this would come from a resource.
                SetEmptyText("No phone numbers");

                // We have a menu item to show in action bar.
                SetHasOptionsMenu(true);

                // Create an empty adapter we will use to display the loaded data.
                mAdapter = new SimpleCursorAdapter(GetActivity(),
                        global::Android.R.Layout.Simple_list_item_1, null,
                        new string[] { Contacts.IPeopleColumnsConstants.DISPLAY_NAME },
                        new int[] { global::Android.R.Id.Text1}, 0);
                SetListAdapter(mAdapter);

                // Start out with a progress indicator.
                SetListShown(false);

                // Prepare the loader.  Either re-connect with an existing one,
                // or start a new one.
                GetLoaderManager().InitLoader(0, null, this);
            }

            public override void OnCreateOptionsMenu(IMenu menu, MenuInflater Inflater) {
                // Place an action bar item for searching.
                IMenuItem item = menu.Add("Search");
                item.SetIcon(global::Android.R.Drawable.Ic_menu_search);
                MenuItemCompat.SetShowAsAction(item, MenuItemCompat.SHOW_AS_ACTION_ALWAYS
                        | MenuItemCompat.SHOW_AS_ACTION_COLLAPSE_ACTION_VIEW);
                View searchView = SearchViewCompat.NewSearchView(GetActivity());
                if (searchView != null) {
                    SearchViewCompat.SetOnQueryTextListener(searchView, new MyOnQueryTextListenerCompat(this));

                    SearchViewCompat.SetOnCloseListener(searchView, new MyOnCloseListenerCompat(searchView));

                    MenuItemCompat.SetActionView(item, searchView);
                }
            }

            public override void OnListItemClick(global::Android.Widget.ListView l, View v, int position, long id)
            {
                // Insert desired behavior here.
                Log.I("FragmentComplexList", "Item clicked: " + id);
            }

            // These are the Contacts rows that we will retrieve.
            static string[] CONTACTS_SUMMARY_PROJECTION = new string[] {
                IBaseColumnsConstants._ID,
                Contacts.IPeopleColumnsConstants.DISPLAY_NAME,
            };

            public Loader<ICursor> OnCreateLoader(int id, Bundle args) {
                // This is called when a new Loader needs to be created.  This
                // sample only has one Loader, so we don't care about the ID.
                // First, pick the base URI to use depending on whether we are
                // currently filtering.
                Uri baseUri;
                if (mCurFilter != null) {
                    baseUri = Uri.WithAppendedPath(Contacts.People.CONTENT_FILTER_URI, Uri.Encode(mCurFilter));
                } else {
                    baseUri = Contacts.People.CONTENT_URI;
                }

                // Now create and return a CursorLoader that will take care of
                // creating a Cursor for the data being displayed.
                string select = "((" + Contacts.IPeopleColumnsConstants.DISPLAY_NAME + " NOTNULL) AND ("
                        + Contacts.IPeopleColumnsConstants.DISPLAY_NAME + " != '' ))";
                return new CursorLoader(GetActivity(), baseUri,
                        CONTACTS_SUMMARY_PROJECTION, select, null,
                        Contacts.IPeopleColumnsConstants.DISPLAY_NAME + " COLLATE LOCALIZED ASC");
            }

            public void OnLoadFinished(Loader<ICursor> loader, ICursor data) {
                // Swap the new cursor in.  (The framework will take care of closing the
                // old cursor once we return.)
                mAdapter.SwapCursor(data);

                // The list should now be shown.
                if (IsResumed()) {
                    SetListShown(true);
                } else {
                    SetListShownNoAnimation(true);
                }
            }

            public void OnLoaderReset(Loader<ICursor> loader) {
                // This is called when the last Cursor provided to onLoadFinished()
                // above is about to be closed.  We need to make sure we are no
                // longer using it.
                mAdapter.SwapCursor(null);
            }
        }

    }
}