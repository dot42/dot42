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
using Android.Support.V4.View;

using Android.Os;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstrates how fragments can participate in the options menu.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_menu_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentMenuSupport : FragmentActivity {
        Fragment mFragment1;
        Fragment mFragment2;
        CheckBox mCheckBox1;
        CheckBox mCheckBox2;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_menu);

            // Make sure the two menu fragments are created.
            FragmentManager fm = GetSupportFragmentManager();
            FragmentTransaction ft = fm.BeginTransaction();
            mFragment1 = fm.FindFragmentByTag("f1");
            if (mFragment1 == null) {
                mFragment1 = new MenuFragment();
                ft.Add(mFragment1, "f1");
            }
            mFragment2 = fm.FindFragmentByTag("f2");
            if (mFragment2 == null) {
                mFragment2 = new Menu2Fragment();
                ft.Add(mFragment2, "f2");
            }
            ft.Commit();

            // Watch check box clicks.
            mCheckBox1 = (CheckBox)FindViewById(R.Ids.menu1);
            mCheckBox1.Click += (o,a) => UpdateFragmentVisibility();
            mCheckBox2 = (CheckBox)FindViewById(R.Ids.menu2);
            mCheckBox2.Click += (o,a) => UpdateFragmentVisibility();

            // Make sure fragments start out with correct visibility.
            UpdateFragmentVisibility();
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState) {
            base.OnRestoreInstanceState(savedInstanceState);
            // Make sure fragments are updated after check box view state is restored.
            UpdateFragmentVisibility();
        }

        // Update fragment visibility based on current check box state.
        void UpdateFragmentVisibility() {
            FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
            if (mCheckBox1.IsChecked()) ft.Show(mFragment1);
            else ft.Hide(mFragment1);
            if (mCheckBox2.IsChecked()) ft.Show(mFragment2);
            else ft.Hide(mFragment2);
            ft.Commit();
        }

        /**
         * A fragment that displays a menu.  This fragment happens to not
         * have a UI (it does not implement OnCreateView), but it could also
         * have one if it wanted.
         */
        public class MenuFragment : Fragment {

            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
                SetHasOptionsMenu(true);
            }

            public override void OnCreateOptionsMenu(IMenu menu, MenuInflater Inflater) {
                IMenuItem item;
                item = menu.Add("Menu 1a");
                MenuItemCompat.SetShowAsAction(item, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM);
                item = menu.Add("Menu 1b");
                MenuItemCompat.SetShowAsAction(item, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM);
            }
        }

        /**
         * Second fragment with a menu.
         */
        public class Menu2Fragment : Fragment {

            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
                SetHasOptionsMenu(true);
            }

            public override void OnCreateOptionsMenu(IMenu menu, MenuInflater Inflater) {
                IMenuItem item;
                item = menu.Add("Menu 2");
                MenuItemCompat.SetShowAsAction(item, MenuItemCompat.SHOW_AS_ACTION_IF_ROOM);
            }
        }
    }
}