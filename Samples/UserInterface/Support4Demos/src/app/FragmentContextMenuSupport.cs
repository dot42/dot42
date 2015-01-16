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

using Android.Os;
using Android.Support.V4.App;
using Android.Util;
using Android.View;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of displaying a context menu from a fragment.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_context_menu_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentContextMenuSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Create the list fragment and Add it as our sole content.
            ContextMenuFragment content = new ContextMenuFragment();
            GetSupportFragmentManager().BeginTransaction().Add(
                    global::Android.R.Id.Content, content).Commit();
        }

        public class ContextMenuFragment : Fragment {

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View root = Inflater.Inflate(R.Layouts.fragment_context_menu, container, false);
                RegisterForContextMenu(root.FindViewById(R.Ids.long_press));
                return root;
            }

            public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenu_IContextMenuInfo menuInfo) {
                base.OnCreateContextMenu(menu, v, menuInfo);
                menu.Add(IMenuConstants.NONE, R.Ids.a_item, IMenuConstants.NONE, "Menu A");
                menu.Add(IMenuConstants.NONE, R.Ids.b_item, IMenuConstants.NONE, "Menu B");
            }

            public override bool OnContextItemSelected(IMenuItem item) {
                switch (item.GetItemId()) {
                    case R.Ids.a_item:
                        Log.I("ContextMenu", "Item 1a was chosen");
                        return true;
                    case R.Ids.b_item:
                        Log.I("ContextMenu", "Item 1b was chosen");
                        return true;
                }
                return base.OnContextItemSelected(item);
            }
        }
    }
}