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

using Android.Content;
using Android.Database;
using Android.Net;
using Android.Os;
using Android.Util;

using System.IO;

using Dot42.Manifest;

namespace com.example.android.supportv4.content
{
    /**
     * This simple ContentProvider provides access to the two example files shared
     * by the ShareCompat example {@link com.example.android.supportv4.App.SharingSupport}.
     */
    [Provider(Authorities= new[]{ "com.example.supportv4.content.sharingsupportprovider" })]
    public class SharingSupportProvider : ContentProvider {
        public static Uri CONTENT_URI =
                Uri.Parse("content://com.example.supportv4.Content.sharingsupportprovider");

        private const string TAG = "SharingSupportProvider";

        public override bool OnCreate() {
            return true;
        }

        public override ICursor Query(Uri uri, string[] projection, string selection, string[] selectionArgs,
                string sortOrder) {
            return null;
        }

        public override string GetType(Uri uri) {
            if (uri.Equals(Uri.WithAppendedPath(CONTENT_URI, "foo.txt")) ||
                    uri.Equals(Uri.WithAppendedPath(CONTENT_URI, "bar.txt"))) {
                return "text/plain";
            }
            return null;
        }

        public override Uri Insert(Uri uri, ContentValues values) {
            return null;
        }

        public override int Delete(Uri uri, string selection, string[] selectionArgs) {
            return 0;
        }

        public override int Update(Uri uri, ContentValues values, string selection, string[] selectionArgs) {
            return 0;
        }

        public override ParcelFileDescriptor OpenFile(Uri uri, string mode) {
            string path = uri.GetPath();
            if (mode.Equals("r") &&
                    (path.Equals("/foo.txt") || path.Equals("/bar.txt"))) {
                try {
                    return ParcelFileDescriptor.Open(
                            new Java.Io.File(GetContext().GetFilesDir() + path),
                            ParcelFileDescriptor.MODE_READ_ONLY);
                } catch (FileNotFoundException) {
                    Log.E(TAG, "Bad file " + uri);
                }
            }
            return null;
        }
    }
}