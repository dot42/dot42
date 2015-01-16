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
using Android.View;

namespace com.example.android.supportv4.app
{
    public class FragmentTabsFragmentSupport : Fragment {
        private FragmentTabHost mTabHost;

        public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                Bundle savedInstanceState) {
            mTabHost = new FragmentTabHost(GetActivity());
            mTabHost.Setup(GetActivity(), GetChildFragmentManager(), R.Ids.fragment1);

            mTabHost.AddTab(mTabHost.NewTabSpec("simple").SetIndicator("Simple"),
                    typeof(FragmentStackSupport.CountingFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("contacts").SetIndicator("Contacts"),
                    typeof(LoaderCursorSupport.CursorLoaderListFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("custom").SetIndicator("Custom"),
                    typeof(LoaderCustomSupport.AppListFragment), null);
            mTabHost.AddTab(mTabHost.NewTabSpec("throttle").SetIndicator("Throttle"),
                    typeof(LoaderThrottleSupport.ThrottledLoaderListFragment), null);

            return mTabHost;
        }

        public override void OnDestroyView() {
            base.OnDestroyView();
            mTabHost = null;
        }
    }
}
