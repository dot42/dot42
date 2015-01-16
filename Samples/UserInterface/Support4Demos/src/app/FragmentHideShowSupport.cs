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

using Android.Os;
using Android.View;
using Android.Widget;

using Dot42;
using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of hiding and showing fragments.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_hide_show_support", WindowSoftInputMode = WindowSoftInputModes.StateUnchanged)]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentHideShowSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_hide_show_support);

            // The content view embeds two fragments; now retrieve them and attach
            // their "hide" button.
            FragmentManager fm = GetSupportFragmentManager();
            AddShowHideListener(R.Ids.frag1hide, fm.FindFragmentById(R.Ids.fragment1));
            AddShowHideListener(R.Ids.frag2hide, fm.FindFragmentById(R.Ids.fragment2));
        }

        void AddShowHideListener(int buttonId, Fragment fragment) {
            Button button = (Button)FindViewById(buttonId);
            button.Click += (o, a) =>
            {
                FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
                ft.SetCustomAnimations(global::Android.R.Anim.Fade_in,
                        global::Android.R.Anim.Fade_out);
                if (fragment.IsHidden())
                {
                    ft.Show(fragment);
                    button.SetText("Hide");
                }
                else
                {
                    ft.Hide(fragment);
                    button.SetText("Show");
                }
                ft.Commit();
            };
        }

		[CustomView]
        public class FirstFragment : Fragment {
            TextView mTextView;

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.labeled_text_edit, container, false);
                View tv = v.FindViewById(R.Ids.msg);
                ((TextView)tv).SetText("The fragment saves and restores this text.");

                // Retrieve the text editor, and restore the last saved state if needed.
                mTextView = (TextView)v.FindViewById(R.Ids.saved);
                if (savedInstanceState != null) {
                    mTextView.SetText(savedInstanceState.GetCharSequence("text"));
                }
                return v;
            }

            public override void OnSaveInstanceState(Bundle outState) {
                base.OnSaveInstanceState(outState);

                // Remember the current text, to restore if we later restart.
                outState.PutCharSequence("text", mTextView.GetText());
            }
        }

		[CustomView]
        public class SecondFragment : Fragment {

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.labeled_text_edit, container, false);
                View tv = v.FindViewById(R.Ids.msg);
                ((TextView)tv).SetText("The TextView saves and restores this text.");

                // Retrieve the text editor and tell it to save and restore its state.
                // Note that you will often set this in the layout XML, but since
                // we are sharing our layout with the other fragment we will customize
                // it here.
                ((TextView)v.FindViewById(R.Ids.saved)).SetSaveEnabled(true);
                return v;
            }
        }
    }
}