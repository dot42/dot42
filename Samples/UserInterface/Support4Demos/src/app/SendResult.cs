/*
 * Copyright (C) 2007 The Android Open Source Project
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

// Need the following using to get access to the app resources, since this
// class is in a sub-package.
using Support4Demos;

using Android.App;
using Android.Content;
using Android.Os;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Example of receiving a result from another activity.
     */
    [Activity(VisibleInLauncher = false, Theme="@style/ThemeDialogWhenLarge")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class SendResult : Activity
    {
        /**
         * Initialization of the Activity after it is first created.  Must at least
         * call {@link Android.App.Activity#SetContentView SetContentView()} to
         * describe what is to be displayed in the screen.
         */
	    protected override void OnCreate(Bundle savedInstanceState)
        {
            // Be sure to call the base.class.
            base.OnCreate(savedInstanceState);

            // See assets/res/any/layout/hello_world.xml for this
            // view layout definition, which is being set here as
            // the content of our screen.
            SetContentView(R.Layouts.send_result);

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.corky);
            button.Click += (o, a) =>
            {
                // To send a result, simply call setResult() before your
                // activity is Finished.
                SetResult(RESULT_OK, (new Intent()).SetAction("Corky!"));
                Finish();
            };
            button = (Button)FindViewById(R.Ids.violet);
            button.Click += (o, a) => {
                // To send a result, simply call setResult() before your
                // activity is Finished.
                SetResult(RESULT_OK, (new Intent()).SetAction("Violet!"));
                Finish();
            };
        }
    }
}
