/*
 * Copyright (C) 2012 The Android Open Source Project
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

using Android.Os;
using Android.Support.V4.App;
using Android.View;
using Android.Widget;

using Support4Demos;

namespace com.example.android.supportv4.app
{
    public class FragmentStackFragmentSupport : Fragment {
        int mStackLevel = 1;

        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState == null) {
                // Do first time initialization -- Add initial fragment.
                Fragment newFragment = FragmentStackSupport.CountingFragment.NewInstance(mStackLevel);
                FragmentTransaction ft = GetChildFragmentManager().BeginTransaction();
                ft.Add(R.Ids.simple_fragment, newFragment).Commit();
            } else {
                mStackLevel = savedInstanceState.GetInt("level");
            }
        }

        public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                Bundle savedInstanceState) {
            View v = Inflater.Inflate(R.Layouts.fragment_stack, container, false);

            // Watch for button clicks.
            Button button = (Button)v.FindViewById(R.Ids.new_fragment);
            button.Click += (o,a) => AddFragmentToStack();
           
            button = (Button)v.FindViewById(R.Ids.delete_fragment);
            button.Click += (o,a) => GetChildFragmentManager().PopBackStack();
   
            button = (Button)v.FindViewById(R.Ids.home);
            button.Click += (o, a) =>
            {
                // If there is a back stack, pop it all.
                FragmentManager fm = GetChildFragmentManager();
                if (fm.GetBackStackEntryCount() > 0)
                {
                    fm.PopBackStack(fm.GetBackStackEntryAt(0).GetId(),
                            FragmentManager.POP_BACK_STACK_INCLUSIVE);
                };
            };

            return v;
        }

        public override void OnSaveInstanceState(Bundle outState) {
            base.OnSaveInstanceState(outState);
            outState.PutInt("level", mStackLevel);
        }

        void AddFragmentToStack() {
            mStackLevel++;

            // Instantiate a new fragment.
            Fragment newFragment = FragmentStackSupport.CountingFragment.NewInstance(mStackLevel);

            // Add the fragment to the activity, pushing this transaction
            // on to the back stack.
            FragmentTransaction ft = GetChildFragmentManager().BeginTransaction();
            ft.Replace(R.Ids.simple_fragment, newFragment);
            ft.SetTransition(FragmentTransaction.TRANSIT_FRAGMENT_OPEN);
            ft.AddToBackStack(null);
            ft.Commit();
        }
    }
}