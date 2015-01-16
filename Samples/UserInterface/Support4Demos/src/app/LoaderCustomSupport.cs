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

using Android_Content = Android.Content;
using Android.Content.Pm;
using Android.Content.Res;
using Android.Graphics.Drawable;
using Android.Os;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Content.Pm;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Util;
using Android.View;
using Android.Widget;

using Support4Demos;

using Java.Io;
using Java.Text;
using Java.Util;
using Java.Lang;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of the implementation of a custom Loader.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/loader_custom_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class LoaderCustomSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            FragmentManager fm = GetSupportFragmentManager();

            // Create the list fragment and Add it as our sole content.
            if (fm.FindFragmentById(global::Android.R.Id.Content) == null) {
                AppListFragment list = new AppListFragment();
                fm.BeginTransaction().Add(global::Android.R.Id.Content, list).Commit();
            }
        }


        /**
         * This class holds the per-item data in our Loader.
         */
        public class AppEntry {
            public AppEntry(AppListLoader loader, ApplicationInfo info) {
                mLoader = loader;
                mInfo = info;
                mApkFile = new File(info.SourceDir);
            }

            public ApplicationInfo GetApplicationInfo() {
                return mInfo;
            }

            public string GetLabel() {
                return mLabel;
            }

            public Drawable GetIcon() {
                if (mIcon == null) {
                    if (mApkFile.Exists()) {
                        mIcon = mInfo.LoadIcon(mLoader.mPm);
                        return mIcon;
                    } else {
                        mMounted = false;
                    }
                } else if (!mMounted) {
                    // If the app wasn't mounted but is now mounted, reload
                    // its icon.
                    if (mApkFile.Exists()) {
                        mMounted = true;
                        mIcon = mInfo.LoadIcon(mLoader.mPm);
                        return mIcon;
                    }
                } else {
                    return mIcon;
                }

                return mLoader.GetContext().GetResources().GetDrawable(
                        global::Android.R.Drawable.Sym_def_app_icon);
            }

            public override string ToString() {
                return mLabel;
            }

            internal void LoadLabel(Android_Content.Context context)
            {
                if (mLabel == null || !mMounted) {
                    if (!mApkFile.Exists()) {
                        mMounted = false;
                        mLabel = mInfo.PackageName;
                    } else {
                        mMounted = true;
                        ICharSequence label = mInfo.LoadLabel(context.GetPackageManager());
                        mLabel = label != null ? label.ToString() : mInfo.PackageName;
                    }
                }
            }

            private readonly AppListLoader mLoader;
            private readonly ApplicationInfo mInfo;
            private readonly File mApkFile;
            private string mLabel;
            private Drawable mIcon;
            private bool mMounted;
        }

        public class MyComparer : IComparator<AppEntry>
        {
            private Collator sCollator = Collator.GetInstance();
            
            public int Compare(AppEntry object1, AppEntry object2) {
                return sCollator.Compare(object1.GetLabel(), object2.GetLabel());
            }
        }

        /**
         * Perform alphabetical comparison of application entry objects.
         */
        public static IComparator<AppEntry> ALPHA_COMPARATOR = new MyComparer();

        /**
         * Helper for determining if the configuration has changed in an interesting
         * way so we need to rebuild the app list.
         */
        public class InterestingConfigChanges {
            readonly Configuration mLastConfiguration = new Configuration();
            int mLastDensity;

            internal bool ApplyNewConfig(Resources res) {
                int configChanges = mLastConfiguration.UpdateFrom(res.GetConfiguration());
                bool densityChanged = mLastDensity != res.GetDisplayMetrics().DensityDpi;
                if (densityChanged || (configChanges&(ActivityInfo.CONFIG_LOCALE
                        |ActivityInfoCompat.CONFIG_UI_MODE|ActivityInfo.CONFIG_SCREEN_LAYOUT)) != 0) {
                    mLastDensity = res.GetDisplayMetrics().DensityDpi;
                    return true;
                }
                return false;
            }
        }

        /**
         * Helper class to look for interesting changes to the installed apps
         * so that the loader can be updated.
         */
        public class PackageIntentReceiver : Android_Content.BroadcastReceiver
        {
            readonly AppListLoader mLoader;

            public PackageIntentReceiver(AppListLoader loader) {
                mLoader = loader;
                Android_Content.IntentFilter filter = new Android_Content.IntentFilter(Android_Content.Intent.ACTION_PACKAGE_ADDED);
                filter.AddAction(Android_Content.Intent.ACTION_PACKAGE_REMOVED);
                filter.AddAction(Android_Content.Intent.ACTION_PACKAGE_CHANGED);
                filter.AddDataScheme("package");
                mLoader.GetContext().RegisterReceiver(this, filter);
                // Register for events related to sdcard installation.
                Android_Content.IntentFilter sdFilter = new Android_Content.IntentFilter();
                sdFilter.AddAction(IntentCompat.ACTION_EXTERNAL_APPLICATIONS_AVAILABLE);
                sdFilter.AddAction(IntentCompat.ACTION_EXTERNAL_APPLICATIONS_UNAVAILABLE);
                mLoader.GetContext().RegisterReceiver(this, sdFilter);
            }

            public override void OnReceive(Android_Content.Context context, Android_Content.Intent intent)
            {
                // Tell the loader about the change.
                mLoader.OnContentChanged();
            }
        }

        /**
         * A custom Loader that loads all of the installed applications.
         */
        public class AppListLoader : AsyncTaskLoader<IList<AppEntry>> {
            readonly InterestingConfigChanges mLastConfig = new InterestingConfigChanges();
            internal readonly PackageManager mPm;

            IList<AppEntry> mApps;
            PackageIntentReceiver mPackageObserver;

            public AppListLoader(Android_Content.Context context)
                : base(context)
            {
                // Retrieve the package manager for later use; note we don't
                // use 'context' directly but instead the save global application
                // context returned by GetContext().
                mPm = GetContext().GetPackageManager();
            }

            /**
             * This is where the bulk of our work is done.  This function is
             * called in a background thread and should generate a new set of
             * data to be published by the loader.
             */
            public override IList<AppEntry> LoadInBackground() {
                // Retrieve all known applications.
                IList<ApplicationInfo> apps = mPm.GetInstalledApplications(
                        PackageManager.GET_UNINSTALLED_PACKAGES |
                        PackageManager.GET_DISABLED_COMPONENTS);
                if (apps == null) {
                    apps = new ArrayList<ApplicationInfo>();
                }

                Android_Content.Context context = GetContext();

                // Create corresponding array of entries and load their labels.
                IList<AppEntry> entries = new ArrayList<AppEntry>(apps.Size());
                for (int i=0; i<apps.Size(); i++) {
                    AppEntry entry = new AppEntry(this, apps.Get(i));
                    entry.LoadLabel(context);
                    entries.Add(entry);
                }

                // Sort the list.
                Collections.Sort(entries, ALPHA_COMPARATOR);

                // Done!
                return entries;
            }

            /**
             * Called when there is new data to deliver to the client.  The
             * base.class will take care of delivering it; the implementation
             * here just Adds a little more logic.
             */
            public override void DeliverResult(IList<AppEntry> apps) {
                if (IsReset()) {
                    // An async query came in while the loader is stopped.  We
                    // don't need the result.
                    if (apps != null) {
                        OnReleaseResources(apps);
                    }
                }
                IList<AppEntry> oldApps = apps;
                mApps = apps;

                if (IsStarted()) {
                    // If the Loader is currently started, we can immediately
                    // deliver its results.
                    base.DeliverResult(apps);
                }

                // At this point we can release the resources associated with
                // 'oldApps' if needed; now that the new result is delivered we
                // know that it is no longer in use.
                if (oldApps != null) {
                    OnReleaseResources(oldApps);
                }
            }

            /**
             * Handles a request to start the Loader.
             */
            protected override void OnStartLoading() {
                if (mApps != null) {
                    // If we currently have a result available, deliver it
                    // immediately.
                    DeliverResult(mApps);
                }

                // Start watching for changes in the app data.
                if (mPackageObserver == null) {
                    mPackageObserver = new PackageIntentReceiver(this);
                }

                // Has something interesting in the configuration changed since we
                // last built the app list?
                bool configChange = mLastConfig.ApplyNewConfig(GetContext().GetResources());

                if (TakeContentChanged() || mApps == null || configChange) {
                    // If the data has changed since the last time it was loaded
                    // or is not currently available, start a load.
                    ForceLoad();
                }
            }

            /**
             * Handles a request to stop the Loader.
             */
            protected override void OnStopLoading() {
                // Attempt to cancel the current load task if possible.
                CancelLoad();
            }

            /**
             * Handles a request to cancel a load.
             */
            public override void OnCanceled(IList<AppEntry> apps) {
                base.OnCanceled(apps);

                // At this point we can release the resources associated with 'apps'
                // if needed.
                OnReleaseResources(apps);
            }

            /**
             * Handles a request to completely reset the Loader.
             */
            protected override void OnReset() {
                base.OnReset();

                // Ensure the loader is stopped
                OnStopLoading();

                // At this point we can release the resources associated with 'apps'
                // if needed.
                if (mApps != null) {
                    OnReleaseResources(mApps);
                    mApps = null;
                }

                // Stop monitoring for changes.
                if (mPackageObserver != null) {
                    GetContext().UnregisterReceiver(mPackageObserver);
                    mPackageObserver = null;
                }
            }

            /**
             * Helper function to take care of releasing resources associated
             * with an actively loaded data set.
             */
            protected void OnReleaseResources(IList<AppEntry> apps) {
                // For a simple List<> there is nothing to do.  For something
                // like a Cursor, we would close it here.
            }
        }



        public class AppListAdapter : ArrayAdapter<AppEntry> {
            private readonly LayoutInflater mInflater;

            public AppListAdapter(Android_Content.Context context)
                : base(context, global::Android.R.Layout.Simple_list_item_2) 
            {
                mInflater = (LayoutInflater)context.GetSystemService(Android_Content.Context.LAYOUT_INFLATER_SERVICE);
            }

            public void SetData(IList<AppEntry> data) {
                Clear();
                if (data != null) 
                {
                    IIterator<AppEntry> it = data.Iterator();
                    while (it.HasNext())
                    {
                        AppEntry appEntry = it.Next();
                        Add(appEntry);
                    }
                }
            }

            /**
             * Populate new items in the list.
             */
            public override View GetView(int position, View convertView, ViewGroup parent) {
                View view;

                if (convertView == null) {
                    view = mInflater.Inflate(R.Layouts.list_item_icon_text, parent, false);
                } else {
                    view = convertView;
                }

                AppEntry item = GetItem(position);
                ((ImageView)view.FindViewById(R.Ids.icon)).SetImageDrawable(item.GetIcon());
                ((TextView)view.FindViewById(R.Ids.text)).SetText(item.GetLabel());

                return view;
            }
        }

        public class AppListFragment : ListFragment, LoaderManager.ILoaderCallbacks<IList<AppEntry>> 
        {
            class MyOnQueryTextListenerCompat : SearchViewCompat.OnQueryTextListenerCompat 
            {
                AppListFragment appListFragment;

                public MyOnQueryTextListenerCompat(AppListFragment appListFragment)
                {
                    this.appListFragment = appListFragment;
                }

                public override bool OnQueryTextChange(string newText) 
                {
                    // Called when the action bar search text has changed.  Since this
                    // is a simple array adapter, we can just have it do the filtering.
                    appListFragment.mCurFilter = !TextUtils.IsEmpty(newText) ? newText : null;
                    appListFragment.mAdapter.GetFilter().JavaFilter(appListFragment.mCurFilter);
                    return true;
                }
            }

            class  MyOnCloseListenerCompat : SearchViewCompat.OnCloseListenerCompat 
            {
                View searchView;

                public MyOnCloseListenerCompat(View searchView)
                {
                    this.searchView = searchView;
                }

                public override bool OnClose() 
                {
                    if (!TextUtils.IsEmpty(SearchViewCompat.GetQuery(searchView))) {
                        SearchViewCompat.SetQuery(searchView, null, true);
                    }
                    return true;
                }
            }

            // This is the Adapter being used to display the list's data.
            AppListAdapter mAdapter;

            // If non-null, this is the current filter the user has provided.
            string mCurFilter;

            //OnQueryTextListenerCompat mOnQueryTextListenerCompat;

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);

                // Give some text to display if there is no data.  In a real
                // application this would come from a resource.
                SetEmptyText("No applications");

                // We have a menu item to show in action bar.
                SetHasOptionsMenu(true);

                // Create an empty adapter we will use to display the loaded data.
                mAdapter = new AppListAdapter(GetActivity());
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
                MenuItemCompat.SetShowAsAction(item, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM
                        | MenuItemCompat.SHOW_AS_ACTION_COLLAPSE_ACTION_VIEW);
                View searchView = SearchViewCompat.NewSearchView(GetActivity());
                if (searchView != null) {
                    SearchViewCompat.SetOnQueryTextListener(searchView, new MyOnQueryTextListenerCompat(this) );
                    SearchViewCompat.SetOnCloseListener(searchView, new MyOnCloseListenerCompat(searchView));
                    MenuItemCompat.SetActionView(item, searchView);
                }
            }

            public override void OnListItemClick(ListView l, View v, int position, long id) {
                // Insert desired behavior here.
                Log.I("LoaderCustom", "Item clicked: " + id);
            }

            public Loader<IList<AppEntry>> OnCreateLoader(int id, Bundle args) {
                // This is called when a new Loader needs to be created.  This
                // sample only has one Loader with no arguments, so it is simple.
                return new AppListLoader(GetActivity());
            }

            public void OnLoadFinished(Loader<IList<AppEntry>> loader, IList<AppEntry> data) {
                // Set the new data in the adapter.
                mAdapter.SetData(data);

                // The list should now be shown.
                if (IsResumed()) {
                    SetListShown(true);
                } else {
                    SetListShownNoAnimation(true);
                }
            }

            public void OnLoaderReset(Loader<IList<AppEntry>> loader) {
                // Clear the data in the adapter.
                mAdapter.SetData(null);
            }
        }

    }
}