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

using Support4Demos;

using Android.Support.V4.App;

using Android.Os;
using Android.View;
using Android.Widget;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstrates how fragments can participate in the options menu.
     */
    public class FragmentMenuFragmentSupport : Fragment {
        Fragment mFragment1;
        Fragment mFragment2;
        CheckBox mCheckBox1;
        CheckBox mCheckBox2;

        public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                Bundle savedInstanceState) {
            View v = Inflater.Inflate(R.Layouts.fragment_menu, container, false);

            // Make sure the two menu fragments are created.
            FragmentManager fm = GetChildFragmentManager();
            FragmentTransaction ft = fm.BeginTransaction();
            mFragment1 = fm.FindFragmentByTag("f1");
            if (mFragment1 == null) {
                mFragment1 = new FragmentMenuSupport.MenuFragment();
                ft.Add(mFragment1, "f1");
            }
            mFragment2 = fm.FindFragmentByTag("f2");
            if (mFragment2 == null) {
                mFragment2 = new FragmentMenuSupport.Menu2Fragment();
                ft.Add(mFragment2, "f2");
            }
            ft.Commit();
        
            // Watch check box clicks.
            mCheckBox1 = (CheckBox)v.FindViewById(R.Ids.menu1);
            mCheckBox1.Click += (o, a) => UpdateFragmentVisibility();
            mCheckBox2 = (CheckBox)v.FindViewById(R.Ids.menu2);
            mCheckBox2.Click += (o, a) => UpdateFragmentVisibility();
        
            // Make sure fragments start out with correct visibility.
            UpdateFragmentVisibility();

            return v;
        }

        public override void OnViewStateRestored(Bundle savedInstanceState) {
            base.OnViewStateRestored(savedInstanceState);
            // Make sure fragments are updated after check box view state is restored.
            UpdateFragmentVisibility();
        }

        // Update fragment visibility based on current check box state.
        void UpdateFragmentVisibility() {
            FragmentTransaction ft = GetChildFragmentManager().BeginTransaction();
            if (mCheckBox1.IsChecked()) ft.Show(mFragment1);
            else ft.Hide(mFragment1);
            if (mCheckBox2.IsChecked()) ft.Show(mFragment2);
            else ft.Hide(mFragment2);
            ft.Commit();
        }
    }
}