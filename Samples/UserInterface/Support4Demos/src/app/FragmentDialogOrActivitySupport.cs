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

using Support4Demos;

using Android.Os;
using Android.Support.V4.App;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_dialog_or_activity_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentDialogOrActivitySupport : FragmentActivity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_dialog_or_activity);

            if (savedInstanceState == null) {
                // First-time init; create fragment to embed in activity.

                FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
                DialogFragment newFragment = MyDialogFragment.NewInstance();
                ft.Add(R.Ids.embedded, newFragment);
                ft.Commit();

            }

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.show_dialog);
            button.Click += (x,y) => ShowDialog();
        }


        void ShowDialog() {
            // Create the fragment and show it as a dialog.
            DialogFragment newFragment = MyDialogFragment.NewInstance();
            newFragment.Show(GetSupportFragmentManager(), "dialog");
        }



        public class MyDialogFragment : DialogFragment {
            internal static MyDialogFragment NewInstance() {
                return new MyDialogFragment();
            }

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.hello_world, container, false);
                View tv = v.FindViewById(R.Ids.text);
                ((TextView)tv).SetText("This is an instance of MyDialogFragment");
                return v;
            }
        }

    }
}