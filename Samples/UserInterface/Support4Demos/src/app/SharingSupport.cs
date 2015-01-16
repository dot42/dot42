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

using Support4Demos;
using com.example.android.supportv4;
using com.example.android.supportv4.content;

using Android.App;
using Android.Net;
using Android.Os;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.View;

using Java.Io;

using System.IO;

using Dot42;
using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * This example illustrates the use of the ShareCompat feature of the support library.
     * ShareCompat offers several pieces of functionality to assist in sharing content between
     * apps and is especially suited for sharing content to social apps that the user has installed.
     *
     * <p>Two other classes are relevant to this code sample: {@link SharingReceiverSupport} is
     * an activity that has been configured to receive ACTION_SEND and ACTION_SEND_MULTIPLE
     * sharing intents with a type of text/plain. It provides an example of writing a sharing
     * target using ShareCompat features. {@link SharingSupportProvider} is a simple
     * {@link Android.Content.ContentProvider} that provides access to two text files
     * created by this app to share as content streams.</p>
     */
    [Activity(VisibleInLauncher = false, Label = "@string/sharing_support_title")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class SharingSupport : Activity {
        protected override void OnCreate(Bundle b) {
            base.OnCreate(b);
            SetContentView(R.Layouts.sharing_support);
        }

        public override bool OnCreateOptionsMenu(IMenu menu) {
            ShareCompat.IntentBuilder b = ShareCompat.IntentBuilder.From(this);
            b.SetType("text/plain").SetText("Share from menu");
            IMenuItem item = menu.Add("Share");
            ShareCompat.ConfigureMenuItem(item, b);
            MenuItemCompat.SetShowAsAction(item, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM);
            return true;
        }

		[EventHandler]
        public void OnShareTextClick(View v) {
            ShareCompat.IntentBuilder.From(this)
                    .SetType("text/plain")
                    .SetText("I'm sharing!")
                    .StartChooser();
        }

		[EventHandler]
        public void OnShareFileClick(View v) {
            try {
                // This file will be accessed by the target of the share through
                // the ContentProvider SharingSupportProvider.
                FileWriter fw = new FileWriter(GetFilesDir() + "/foo.txt");
                fw.Write("This is a file share");
                fw.Close();

                ShareCompat.IntentBuilder.From(this)
                        .SetType("text/plain")
                        .SetStream(Uri.Parse(SharingSupportProvider.CONTENT_URI + "/foo.txt"))
                        .StartChooser();
            } catch (FileNotFoundException e) {
                e.PrintStackTrace();
            } catch (IOException e) {
                e.PrintStackTrace();
            }
        }

		[EventHandler]
        public void OnShareMultipleFileClick(View v) {
            try {
                // These files will be accessed by the target of the share through
                // the ContentProvider SharingSupportProvider.
                FileWriter fw = new FileWriter(GetFilesDir() + "/foo.txt");
                fw.Write("This is a file share");
                fw.Close();

                fw = new FileWriter(GetFilesDir() + "/bar.txt");
                fw.Write("This is another file share");
                fw.Close();

                ShareCompat.IntentBuilder.From(this)
                        .SetType("text/plain")
                        .AddStream(Uri.Parse(SharingSupportProvider.CONTENT_URI + "/foo.txt"))
                        .AddStream(Uri.Parse(SharingSupportProvider.CONTENT_URI + "/bar.txt"))
                        .StartChooser();
            } catch (FileNotFoundException e) {
                e.PrintStackTrace();
            } catch (IOException e) {
                e.PrintStackTrace();
            }
        }
    }
}