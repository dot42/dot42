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

using Android.App;
using Android.Graphics.Drawable;
using Android.Net;
using Android.Os;
using Android.Support.V4.App;
using Android.Util;
using Android.Widget;

using Java.Io;

using System.IO;
using System.Text;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * This example shows a simple way to handle data shared with your app through the
     * use of the support library's ShareCompat features. It will display shared text
     * content as well as the application label and icon of the app that shared the content.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/sharing_receiver_title")]
    [IntentFilter(Actions = new[] { 
		"android.intent.action.SEND", "android.intent.action.SEND_MULTIPLE"
	}, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class SharingReceiverSupport : Activity {
        private const string TAG = "SharingReceiverSupport";
        private const int ICON_SIZE = 32; // dip

        protected override void OnCreate(Bundle b) {
            base.OnCreate(b);
            SetContentView(R.Layouts.sharing_receiver_support);

            float density = GetResources().GetDisplayMetrics().Density;
            int iconSize = (int) (ICON_SIZE * density + 0.5f);

            ShareCompat.IntentReader intentReader = ShareCompat.IntentReader.From(this);

            // The following provides attribution for the app that shared the data with us.
            TextView info = (TextView) FindViewById(R.Ids.app_info);
            Drawable d = intentReader.GetCallingActivityIcon();
            d.SetBounds(0, 0, iconSize, iconSize);
            info.SetCompoundDrawables(d, null, null, null);
            info.SetText(intentReader.GetCallingApplicationLabel());

            TextView tv = (TextView) FindViewById(R.Ids.text);
            StringBuilder txt = new StringBuilder("Received share!\nText was: ");

            txt.Append(intentReader.GetText());
            txt.Append("\n");

            txt.Append("Streams included:\n");
            int N = intentReader.GetStreamCount();
            for (int i = 0; i < N; i++) {
                Uri uri = intentReader.GetStream(i);
                txt.Append("Share included stream " + i + ": " + uri + "\n");
                try {
                    BufferedReader reader = new BufferedReader(new InputStreamReader(
                            GetContentResolver().OpenInputStream(uri)));
                    try {
                        txt.Append(reader.ReadLine() + "\n");
                    } catch (IOException e) {
                        Log.E(TAG, "Reading stream threw exception", e);
                    } finally {
                        reader.Close();
                    }
                } catch (FileNotFoundException e) {
                    Log.E(TAG, "File not found from share.", e);
                } catch (IOException e) {
                    Log.D(TAG, "I/O Error", e);
                }
            }

            tv.SetText(txt.ToString());
        }
    }
}