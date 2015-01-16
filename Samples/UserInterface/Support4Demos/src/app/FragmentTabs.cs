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

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * This demonstrates how you can implement switching between the tabs of a
     * TabHost through fragments, using FragmentTabHost.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_tabs")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentTabs : FragmentActivity {
        private FragmentTabHost mTabHost;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            SetContentView(R.Layouts.fragment_tabs);
            mTabHost = (FragmentTabHost)FindViewById(global::Android.R.Id.Tabhost);
            mTabHost.Setup(this, GetSupportFragmentManager(), R.Ids.realtabcontent);

            mTabHost.AddTab(mTabHost.NewTabSpec("simple").SetIndicator("Simple"),
                    typeof(FragmentStackSupport.CountingFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("contacts").SetIndicator("Contacts"),
                    typeof(LoaderCursorSupport.CursorLoaderListFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("custom").SetIndicator("Custom"),
                    typeof(LoaderCustomSupport.AppListFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("throttle").SetIndicator("Throttle"),
                    typeof(LoaderThrottleSupport.ThrottledLoaderListFragment), null);
        }
    }
}
