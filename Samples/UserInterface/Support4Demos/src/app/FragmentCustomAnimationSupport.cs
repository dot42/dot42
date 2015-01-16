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

using Dot42;
using Support4Demos;

using Android.Support.V4.App;

using Android.Os;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_custom_animation_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentCustomAnimationSupport : FragmentActivity {
        int mStackLevel = 1;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_stack);

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.new_fragment);
            button.Click += (x,y) => AddFragmentToStack();

            if (savedInstanceState == null) {
                // Do first time initialization -- Add initial fragment.
                Fragment newFragment = CountingFragment.NewInstance(mStackLevel);
                FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
                ft.Add(R.Ids.simple_fragment, newFragment).Commit();
            } else {
                mStackLevel = savedInstanceState.GetInt("level");
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("level", mStackLevel);
        }


        void AddFragmentToStack() {
            mStackLevel++;

            // Instantiate a new fragment.
            Fragment newFragment = CountingFragment.NewInstance(mStackLevel);

            // Add the fragment to the activity, pushing this transaction
            // on to the back stack.
            FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
            ft.SetCustomAnimations(R.Anims.fragment_slide_left_enter,
                    R.Anims.fragment_slide_left_exit,
                    R.Anims.fragment_slide_right_enter,
                    R.Anims.fragment_slide_right_exit);
            ft.Replace(R.Ids.simple_fragment, newFragment);
            ft.AddToBackStack(null);
            ft.Commit();
        }



        [CustomView]
        public class CountingFragment : Fragment {
            int mNum;

            /**
             * Create a new instance of CountingFragment, providing "num"
             * as an argument.
             */
            internal static CountingFragment NewInstance(int num) {
                CountingFragment f = new CountingFragment();

                // Supply num input as an argument.
                Bundle args = new Bundle();
                args.PutInt("num", num);
                f.SetArguments(args);

                return f;
            }

            /**
             * When creating, retrieve this instance's number from its arguments.
             */
            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
                mNum = GetArguments() != null ? GetArguments().GetInt("num") : 1;
            }

            /**
             * The Fragment's UI is just a simple text view showing its
             * instance number.
             */
            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.hello_world, container, false);
                View tv = v.FindViewById(R.Ids.text);
                ((TextView)tv).SetText("Fragment #" + mNum);
                tv.SetBackgroundDrawable(GetResources().GetDrawable(global::Android.R.Drawable.Gallery_thumb));
                return v;
            }
        }

    }
}