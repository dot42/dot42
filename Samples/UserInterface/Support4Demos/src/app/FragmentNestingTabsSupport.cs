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

using Android.Os;
using Android.Support.V4.App;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_nesting_tabs_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentNestingTabsSupport : FragmentActivity {
        private FragmentTabHost mTabHost;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            mTabHost = new FragmentTabHost(this);
            SetContentView(mTabHost);
            mTabHost.Setup(this, GetSupportFragmentManager(), R.Ids.fragment1);

            mTabHost.AddTab(mTabHost.NewTabSpec("menus").SetIndicator("Menus"),
                    typeof(FragmentMenuFragmentSupport), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("contacts").SetIndicator("Contacts"),
                    typeof(LoaderCursorSupport.CursorLoaderListFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("stack").SetIndicator("Stack"),
                    typeof(FragmentStackFragmentSupport), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("tabs").SetIndicator("Tabs"),
                    typeof(FragmentTabsFragmentSupport), null);
        }
    }
}