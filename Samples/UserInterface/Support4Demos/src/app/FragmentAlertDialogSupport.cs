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

using Android.Support.V4.App;

using Android.Content;
using Android.Os;
using Android.Util;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
    {
    /**
     * Demonstrates how to show an AlertDialog that is managed by a Fragment.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_alert_dialog_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentAlertDialogSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_dialog);

            View tv = FindViewById(R.Ids.text);
            ((TextView)tv).SetText("Example of displaying an alert dialog with a DialogFragment");

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.show);
            button.Click += (x,y) => ShowDialog();
        }


        void ShowDialog() {
            DialogFragment newFragment = MyAlertDialogFragment.NewInstance(
                    R.Strings.alert_dialog_two_buttons_title);
            newFragment.Show(GetSupportFragmentManager(), "dialog");
        }

        public void DoPositiveClick() {
            // Do stuff here.
            Log.I("FragmentAlertDialog", "Positive click!");
        }

        public void DoNegativeClick() {
            // Do stuff here.
            Log.I("FragmentAlertDialog", "Negative click!");
        }



        public class MyAlertDialogFragment : DialogFragment {

            public static MyAlertDialogFragment NewInstance(int title) {
                MyAlertDialogFragment frag = new MyAlertDialogFragment();
                Bundle args = new Bundle();
                args.PutInt("title", title);
                frag.SetArguments(args);
                return frag;
            }

            public override global::Android.App.Dialog OnCreateDialog(Bundle savedInstanceState) {
                int title = GetArguments().GetInt("title");

                return new global::Android.App.AlertDialog.Builder(GetActivity())
                        .SetIcon(R.Drawables.alert_dialog_icon)
                        .SetTitle(title)
                        .SetPositiveButton(R.Strings.alert_dialog_ok, new System.EventHandler<DialogInterfaceClickEventArgs>((o,a)=>{((FragmentAlertDialogSupport)GetActivity()).DoPositiveClick();})) 
                        .SetNegativeButton(R.Strings.alert_dialog_cancel, new System.EventHandler<DialogInterfaceClickEventArgs>((o,a)=>{((FragmentAlertDialogSupport)GetActivity()).DoNegativeClick();}))
                        .Create();
            }
        }

    }
}