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

using Android.Support.V4.App;

using Android.Os;
using Android.Util;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of using ListFragment to show a list of items
     * from a canned array.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_list_array_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentListArraySupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Create the list fragment and Add it as our sole content.
            if (GetSupportFragmentManager().FindFragmentById(global::Android.R.Id.Content) == null) {
                ArrayListFragment list = new ArrayListFragment();
                GetSupportFragmentManager().BeginTransaction().Add(global::Android.R.Id.Content, list).Commit();
            }
        }

        public class ArrayListFragment : ListFragment {

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);
                SetListAdapter(new ArrayAdapter<string>(GetActivity(),
                        global::Android.R.Layout.Simple_list_item_1, Shakespeare.TITLES));
            }

            public override void OnListItemClick(ListView l, View v, int position, long id) {
                Log.I("FragmentList", "Item clicked: " + id);
            }
        }
    }
}