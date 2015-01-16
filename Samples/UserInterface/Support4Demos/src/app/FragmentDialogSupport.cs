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

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_dialog_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentDialogSupport : FragmentActivity {
        int mStackLevel = 0;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_dialog);

            View tv = FindViewById(R.Ids.text);
            ((TextView)tv).SetText("Example of displaying dialogs with a DialogFragment.  "
                    + "Press the show button below to see the first dialog; pressing "
                    + "successive show buttons will display other dialog styles as a "
                    + "stack, with dismissing or back going to the previous dialog.");

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.show);
            button.Click += (x,y) => ShowDialog();

            if (savedInstanceState != null) {
                mStackLevel = savedInstanceState.GetInt("level");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState) {
            base.OnSaveInstanceState(outState);
            outState.PutInt("level", mStackLevel);
        }


        void ShowDialog() {
            mStackLevel++;

            // DialogFragment.Show() will take care of Adding the fragment
            // in a transaction.  We also want to remove any currently showing
            // dialog, so make our own transaction and take care of that here.
            FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
            Fragment prev = GetSupportFragmentManager().FindFragmentByTag("dialog");
            if (prev != null) {
                ft.Remove(prev);
            }
            ft.AddToBackStack(null);

            // Create and show the dialog.
            DialogFragment newFragment = MyDialogFragment.NewInstance(mStackLevel);
            newFragment.Show(ft, "dialog");
        }


        static string GetNameForNum(int num) {
            switch ((num-1)%6) {
                case 1: return "STYLE_NO_TITLE";
                case 2: return "STYLE_NO_FRAME";
                case 3: return "STYLE_NO_INPUT (this window can't receive input, so "
                        + "you will need to press the bottom show button)";
                case 4: return "STYLE_NORMAL with dark fullscreen theme";
                case 5: return "STYLE_NORMAL with light theme";
                case 6: return "STYLE_NO_TITLE with light theme";
                case 7: return "STYLE_NO_FRAME with light theme";
                case 8: return "STYLE_NORMAL with light fullscreen theme";
            }
            return "STYLE_NORMAL";
        }

        public class MyDialogFragment : DialogFragment {
            int mNum;

            /**
             * Create a new instance of MyDialogFragment, providing "num"
             * as an argument.
             */
            internal static MyDialogFragment NewInstance(int num) {
                MyDialogFragment f = new MyDialogFragment();

                // Supply num input as an argument.
                Bundle args = new Bundle();
                args.PutInt("num", num);
                f.SetArguments(args);

                return f;
            }

            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
                mNum = GetArguments().GetInt("num");

                // Pick a style based on the num.
                int style = DialogFragment.STYLE_NORMAL, theme = 0;
                switch ((mNum-1)%6) {
                    case 1: style = DialogFragment.STYLE_NO_TITLE; break;
                    case 2: style = DialogFragment.STYLE_NO_FRAME; break;
                    case 3: style = DialogFragment.STYLE_NO_INPUT; break;
                    case 4: style = DialogFragment.STYLE_NORMAL; break;
                    case 5: style = DialogFragment.STYLE_NO_TITLE; break;
                    case 6: style = DialogFragment.STYLE_NO_FRAME; break;
                    case 7: style = DialogFragment.STYLE_NORMAL; break;
                }
                switch ((mNum-1)%6) {
                    case 2: theme = global::Android.R.Style.Theme_Panel; break;
                    case 4: theme = global::Android.R.Style.Theme; break;
                    case 5: theme = global::Android.R.Style.Theme_Light; break;
                    case 6: theme = global::Android.R.Style.Theme_Light_Panel; break;
                    case 7: theme = global::Android.R.Style.Theme_Light; break;
                }
                SetStyle(style, theme);
            }

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.fragment_dialog, container, false);
                View tv = v.FindViewById(R.Ids.text);
                ((TextView)tv).SetText("Dialog #" + mNum + ": using style "
                        + GetNameForNum(mNum));

                // Watch for button clicks.
                Button button = (Button)v.FindViewById(R.Ids.show);
                button.Click += (o, a) => ((FragmentDialogSupport)GetActivity()).ShowDialog();

                return v;
            }
        }

    }
}