/*
 * Copyright (C) 2011 The Android Open Source Project
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
using Android.Database;
using Android.Database.Sqlite;
using Android.Net;
using Android.Os;
using Android.Provider;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Database;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Util;
using Android.View;

using Java.Util;
using Java.Lang;

using System.Text;
using System.Threading;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of bottom to top implementation of a content provider holding
     * structured data through displaying it in the UI, using throttling to reduce
     * the number of queries done when its data changes.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/loader_throttle_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class LoaderThrottleSupport : FragmentActivity {
        // Debugging.
        static string TAG = "LoaderThrottle";

        /**
         * The authority we use to get to our sample provider.
         */
        public static string AUTHORITY = "com.example.android.apis.supportv4.app.LoaderThrottle";

        /**
         * Definition of the contract for the main table of our provider.
         */
        public sealed class MainTable : IBaseColumns {

            // This class cannot be instantiated
            private MainTable() {}

            /**
             * The table name offered by this provider
             */
            public const string TABLE_NAME = "main";

            /**
             * The content:// style URL for this table
             */
            public static Uri CONTENT_URI = Uri.Parse("content://" + LoaderThrottleSupport.AUTHORITY + "/main");

            /**
             * The content URI base for a single row of data. Callers must
             * append a numeric row id to this Uri to retrieve a row
             */
            public static Uri CONTENT_ID_URI_BASE = Uri.Parse("content://" + LoaderThrottleSupport.AUTHORITY + "/main/");

            /**
             * The MIME type of {@link #CONTENT_URI}.
             */
            public const string CONTENT_TYPE = "vnd.Android.cursor.dir/vnd.example.api-demos-throttle";

            /**
             * The MIME type of a {@link #CONTENT_URI} sub-directory of a single row.
             */
            public const string CONTENT_ITEM_TYPE = "vnd.Android.cursor.item/vnd.example.api-demos-throttle";
            /**
             * The default sort order for this table
             */
            public const string DEFAULT_SORT_ORDER = "data COLLATE LOCALIZED ASC";

            /**
             * Column name for the single column holding our data.
             * <P>Type: TEXT</P>
             */
            public const string COLUMN_NAME_DATA = "data";
        }

        /**
         * This class helps open, create, and upgrade the database file.
         */
       class DatabaseHelper : SQLiteOpenHelper {

           private const string DATABASE_NAME = "loader_throttle.db";
           private const int DATABASE_VERSION = 2;

           public DatabaseHelper(global::Android.Content.Context context) : base(context, DATABASE_NAME, null, DATABASE_VERSION)
           {
               // calls the base.constructor, requesting the default cursor factory.
           }

           /**
            *
            * Creates the underlying database with table name and column names taken from the
            * NotePad class.
            */
           public override void OnCreate(SQLiteDatabase db) {
               db.ExecSQL("CREATE TABLE " + MainTable.TABLE_NAME + " ("
                       + IBaseColumnsConstants._ID + " INTEGER PRIMARY KEY,"
                       + MainTable.COLUMN_NAME_DATA + " TEXT"
                       + ");");
           }

           /**
            *
            * Demonstrates that the provider must consider what happens when the
            * underlying datastore is changed. In this sample, the database is upgraded the database
            * by destroying the existing data.
            * A real application should upgrade the database in place.
            */
           public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {

               // Logs that the database is being upgraded
               Log.W(TAG, "Upgrading database from version " + oldVersion + " to "
                       + newVersion + ", which will destroy all old data");

               // Kills the table and existing data
               db.ExecSQL("DROP TABLE IF EXISTS notes");

               // Recreates the database with a new version
               OnCreate(db);
           }
       }

        /**
         * A very simple implementation of a content provider.
         */
       [Provider(Authorities= new [] { "com.example.android.apis.supportv4.app.LoaderThrottle" })]
       public class SimpleProvider : Android_Content.ContentProvider
       {
            // A projection map used to select columns from the database
            private readonly HashMap<string, string> mNotesProjectionMap;
            // Uri matcher to decode incoming URIs.
            private readonly Android_Content.UriMatcher mUriMatcher;

            // The incoming URI matches the main table URI pattern
            private const int MAIN = 1;
            // The incoming URI matches the main table row ID URI pattern
            private const int MAIN_ID = 2;

            // Handle to a new DatabaseHelper.
            private DatabaseHelper mOpenHelper;

            /**
             * Global provider initialization.
             */
            public SimpleProvider() {
                // Create and initialize URI matcher.
                mUriMatcher = new Android_Content.UriMatcher(Android_Content.UriMatcher.NO_MATCH);
                mUriMatcher.AddURI(AUTHORITY, MainTable.TABLE_NAME, MAIN);
                mUriMatcher.AddURI(AUTHORITY, MainTable.TABLE_NAME + "/#", MAIN_ID);

                // Create and initialize projection map for all columns.  This is
                // simply an identity mapping.
                mNotesProjectionMap = new HashMap<string, string>();
                mNotesProjectionMap.Put(IBaseColumnsConstants._ID, IBaseColumnsConstants._ID);
                mNotesProjectionMap.Put(MainTable.COLUMN_NAME_DATA, MainTable.COLUMN_NAME_DATA);
            }

            /**
             * Perform provider creation.
             */
            public override bool OnCreate() {
                mOpenHelper = new DatabaseHelper(GetContext());
                // Assumes that any failures will be reported by a thrown exception.
                return true;
            }

            /**
             * Handle incoming queries.
             */
            public override ICursor Query(Uri uri, string[] projection, string selection,
                    string[] selectionArgs, string sortOrder) {

                // Constructs a new query builder and sets its table name
                SQLiteQueryBuilder qb = new SQLiteQueryBuilder();
                qb.SetTables(MainTable.TABLE_NAME);

                switch (mUriMatcher.Match(uri)) {
                    case MAIN:
                        // If the incoming URI is for main table.
                        qb.SetProjectionMap(mNotesProjectionMap);
                        break;

                    case MAIN_ID:
                        // The incoming URI is for a single row.
                        qb.SetProjectionMap(mNotesProjectionMap);
                        qb.AppendWhere(IBaseColumnsConstants._ID + "=?");
                        selectionArgs = DatabaseUtilsCompat.AppendSelectionArgs(selectionArgs,
                                new string[] { uri.GetLastPathSegment() });
                        break;

                    default:
                        throw new System.ArgumentException("Unknown URI " + uri);
                }


                if (TextUtils.IsEmpty(sortOrder)) {
                    sortOrder = MainTable.DEFAULT_SORT_ORDER;
                }

                SQLiteDatabase db = mOpenHelper.GetReadableDatabase();

                ICursor c = qb.Query(db, projection, selection, selectionArgs,
                        null /* no group */, null /* no filter */, sortOrder);

                c.SetNotificationUri(GetContext().GetContentResolver(), uri);
                return c;
            }

            /**
             * Return the MIME type for an known URI in the provider.
             */
            public override string GetType(Uri uri) {
                switch (mUriMatcher.Match(uri)) {
                    case MAIN:
                        return MainTable.CONTENT_TYPE;
                    case MAIN_ID:
                        return MainTable.CONTENT_ITEM_TYPE;
                    default:
                        throw new System.ArgumentException("Unknown URI " + uri);
                }
            }

            /**
             * Handler inserting new data.
             */
            public override Uri Insert(Uri uri, Android_Content.ContentValues initialValues)
            {
                if (mUriMatcher.Match(uri) != MAIN) {
                    // Can only insert into to main URI.
                    throw new System.ArgumentException("Unknown URI " + uri);
                }

                Android_Content.ContentValues values;

                if (initialValues != null) {
                    values = new Android_Content.ContentValues(initialValues);
                } else {
                    values = new Android_Content.ContentValues();
                }

                if (values.ContainsKey(MainTable.COLUMN_NAME_DATA) == false) {
                    values.Put(MainTable.COLUMN_NAME_DATA, "");
                }

                SQLiteDatabase db = mOpenHelper.GetWritableDatabase();

                long rowId = db.Insert(MainTable.TABLE_NAME, null, values);

                // If the insert succeeded, the row ID exists.
                if (rowId > 0) {
                    Uri noteUri = Android_Content.ContentUris.WithAppendedId(MainTable.CONTENT_ID_URI_BASE, rowId);
                    GetContext().GetContentResolver().NotifyChange(noteUri, null);
                    return noteUri;
                }

                throw new SQLException("Failed to insert row into " + uri);
            }

            /**
             * Handle deleting data.
             */
            public override int Delete(Uri uri, string where, string[] whereArgs) {
                SQLiteDatabase db = mOpenHelper.GetWritableDatabase();
                string constWhere;

                int count;

                switch (mUriMatcher.Match(uri)) {
                    case MAIN:
                        // If URI is main table, delete uses incoming where clause and args.
                        count = db.Delete(MainTable.TABLE_NAME, where, whereArgs);
                        break;

                        // If the incoming URI matches a single note ID, does the delete based on the
                        // incoming data, but modifies the where clause to restrict it to the
                        // particular note ID.
                    case MAIN_ID:
                        // If URI is for a particular row ID, delete is based on incoming
                        // data but modified to restrict to the given ID.
                        constWhere = DatabaseUtilsCompat.ConcatenateWhere(
                                IBaseColumnsConstants._ID + " = " + Android_Content.ContentUris.ParseId(uri), where);
                        count = db.Delete(MainTable.TABLE_NAME, constWhere, whereArgs);
                        break;

                    default:
                        throw new System.ArgumentException("Unknown URI " + uri);
                }

                GetContext().GetContentResolver().NotifyChange(uri, null);

                return count;
            }

            /**
             * Handle updating data.
             */
            public override int Update(Uri uri, Android_Content.ContentValues values, string where, string[] whereArgs)
            {
                SQLiteDatabase db = mOpenHelper.GetWritableDatabase();
                int count;
                string constWhere;

                switch (mUriMatcher.Match(uri)) {
                    case MAIN:
                        // If URI is main table, update uses incoming where clause and args.
                        count = db.Update(MainTable.TABLE_NAME, values, where, whereArgs);
                        break;

                    case MAIN_ID:
                        // If URI is for a particular row ID, update is based on incoming
                        // data but modified to restrict to the given ID.
                        constWhere = DatabaseUtilsCompat.ConcatenateWhere(
                                IBaseColumnsConstants._ID + " = " + Android_Content.ContentUris.ParseId(uri), where);
                        count = db.Update(MainTable.TABLE_NAME, values, constWhere, whereArgs);
                        break;

                    default:
                        throw new System.ArgumentException("Unknown URI " + uri);
                }

                GetContext().GetContentResolver().NotifyChange(uri, null);

                return count;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            FragmentManager fm = GetSupportFragmentManager();

            // Create the list fragment and Add it as our sole content.
            if (fm.FindFragmentById(global::Android.R.Id.Content) == null) {
                ThrottledLoaderListFragment list = new ThrottledLoaderListFragment();
                fm.BeginTransaction().Add(global::Android.R.Id.Content, list).Commit();
            }
        }

        public class ThrottledLoaderListFragment : ListFragment
                , LoaderManager.ILoaderCallbacks<ICursor> {

            // Menu identifiers
            const int POPULATE_ID = IMenuConstants.FIRST;
            const int CLEAR_ID = IMenuConstants.FIRST+1;

            // This is the Adapter being used to display the list's data.
            SimpleCursorAdapter mAdapter;

            // Task we have running to populate the database.
            AsyncTask<object, object, object> mPopulatingTask;

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);

                SetEmptyText("No data.  Select 'Populate' to fill with data from Z to A at a rate of 4 per second.");
                SetHasOptionsMenu(true);

                // Create an empty adapter we will use to display the loaded data.
                mAdapter = new SimpleCursorAdapter(GetActivity(),
                        global::Android.R.Layout.Simple_list_item_1, null,
                        new string[] { MainTable.COLUMN_NAME_DATA },
                        new int[] { global::Android.R.Id.Text1 }, 0);
                SetListAdapter(mAdapter);

                // Start out with a progress indicator.
                SetListShown(false);

                // Prepare the loader.  Either re-connect with an existing one,
                // or start a new one.
                GetLoaderManager().InitLoader(0, null, this);
            }

            public override void OnCreateOptionsMenu(IMenu menu, MenuInflater Inflater) {
                IMenuItem populateItem = menu.Add(IMenuConstants.NONE, POPULATE_ID, 0, "Populate");
                MenuItemCompat.SetShowAsAction(populateItem, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM);
                IMenuItem clearItem = menu.Add(IMenuConstants.NONE, CLEAR_ID, 0, "Clear");
                MenuItemCompat.SetShowAsAction(clearItem, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM);
            }

            class PopulateAsyncTask : AsyncTask<object, object, object>
            {
                readonly Android_Content.ContentResolver cr;

                public PopulateAsyncTask(Android_Content.ContentResolver cr)
	            {
                    this.cr = cr;
	            }

                protected override object DoInBackground(params object[] @params)
                {
                    for (char c='Z'; c>='A'; c--) {
                        if (IsCancelled()) {
                            break;
                        }
                        StringBuilder builder = new StringBuilder("Data ");
                        builder.Append(c);
                        Android_Content.ContentValues values = new Android_Content.ContentValues();
                        values.Put(MainTable.COLUMN_NAME_DATA, builder.ToString());
                        cr.Insert(MainTable.CONTENT_URI, values);
                        // Wait a bit between each insert.
                        try {
                            Thread.Sleep(250);
                        } catch (InterruptedException) {
                        }
                    }
                    return null;
                }
            }

            class ClearAsyncTask : AsyncTask<object, object, object>
            {
                readonly Android_Content.ContentResolver cr;

                public ClearAsyncTask(Android_Content.ContentResolver cr)
	            {
                    this.cr = cr;
	            }

                protected override object DoInBackground(params object[] @params)
                {
                    cr.Delete(MainTable.CONTENT_URI, null, null);
                    return null;
                }
             }

            public override bool OnOptionsItemSelected(IMenuItem item) {
                Android_Content.ContentResolver cr = GetActivity().GetContentResolver();

                switch (item.GetItemId()) {
                    case POPULATE_ID:
                        if (mPopulatingTask != null) {
                            mPopulatingTask.Cancel(false);
                        }
                        mPopulatingTask = new PopulateAsyncTask(cr);

                        mPopulatingTask.Execute((object[]) null);
                        return true;

                    case CLEAR_ID:
                        if (mPopulatingTask != null) {
                            mPopulatingTask.Cancel(false);
                            mPopulatingTask = null;
                        }
                        AsyncTask<object, object, object> task = new ClearAsyncTask(cr);
                        task.Execute((object[])null);
                        return true;

                    default:
                        return base.OnOptionsItemSelected(item);
                }
            }

            public override void OnListItemClick(global::Android.Widget.ListView l, View v, int position, long id)
            {
                // Insert desired behavior here.
                Log.I(TAG, "Item clicked: " + id);
            }

            // These are the rows that we will retrieve.
            static string[] PROJECTION = new string[] {
                IBaseColumnsConstants._ID,
                MainTable.COLUMN_NAME_DATA,
            };

            public Loader<ICursor> OnCreateLoader(int id, Bundle args) {
                CursorLoader cl = new CursorLoader(GetActivity(), MainTable.CONTENT_URI,
                        PROJECTION, null, null, null);
                cl.SetUpdateThrottle(2000); // update at most every 2 seconds.
                return cl;
            }

            public void OnLoadFinished(Loader<ICursor> loader, ICursor data) {
                mAdapter.SwapCursor(data);

                // The list should now be shown.
                if (IsResumed()) {
                    SetListShown(true);
                } else {
                    SetListShownNoAnimation(true);
                }
            }

            public void OnLoaderReset(Loader<ICursor> loader) {
                mAdapter.SwapCursor(null);
            }
        }
    }

}