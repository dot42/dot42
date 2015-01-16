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

using Android.Os;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_pager_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentPagerSupport : FragmentActivity {
        const int NUM_ITEMS = 10;

        MyAdapter mAdapter;

        ViewPager mPager;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_pager);

            mAdapter = new MyAdapter(GetSupportFragmentManager());

            mPager = (ViewPager)FindViewById(R.Ids.pager);
            mPager.SetAdapter(mAdapter);

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.goto_first);
            button.Click += (o, a) => mPager.SetCurrentItem(0);
           
            button = (Button)FindViewById(R.Ids.goto_last);
            button.Click += (o, a) => mPager.SetCurrentItem(NUM_ITEMS-1);
        }

        public class MyAdapter : FragmentPagerAdapter {
            public MyAdapter(FragmentManager fm) : base(fm) {
            }

            public override int GetCount() {
                return NUM_ITEMS;
            }

            public override Fragment GetItem(int position) {
                return ArrayListFragment.NewInstance(position);
            }
        }

        [CustomView]
        public class ArrayListFragment : ListFragment {
            int mNum;

            /**
             * Create a new instance of CountingFragment, providing "num"
             * as an argument.
             */
            internal static ArrayListFragment NewInstance(int num) {
                ArrayListFragment f = new ArrayListFragment();

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
                View v = Inflater.Inflate(R.Layouts.fragment_pager_list, container, false);
                View tv = v.FindViewById(R.Ids.text);
                ((TextView)tv).SetText("Fragment #" + mNum);
                return v;
            }

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);
                SetListAdapter(new ArrayAdapter<string>(GetActivity(),
                        global::Android.R.Layout.Simple_list_item_1, Cheeses.sCheesestrings));
            }

            public override void OnListItemClick(ListView l, View v, int position, long id) {
                Log.I("FragmentList", "Item clicked: " + id);
            }
        }
    }
}