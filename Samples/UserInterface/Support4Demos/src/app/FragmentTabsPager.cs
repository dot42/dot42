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

using Android.Content;
using Android.Os;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.View;
using Android.Widget;

using Java.Util;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstrates combining a TabHost with a ViewPager to implement a tab UI
     * that switches between tabs and also allows the user to perform horizontal
     * flicks to move between the tabs.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_tabs_pager")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentTabsPager : FragmentActivity {
        TabHost mTabHost;
        ViewPager  mViewPager;
        TabsAdapter mTabsAdapter;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            SetContentView(R.Layouts.fragment_tabs_pager);
            mTabHost = (TabHost)FindViewById(global::Android.R.Id.Tabhost);
            mTabHost.Setup();

            mViewPager = (ViewPager)FindViewById(R.Ids.pager);

            mTabsAdapter = new TabsAdapter(this, mTabHost, mViewPager);

            mTabsAdapter.AddTab(mTabHost.NewTabSpec("simple").SetIndicator("Simple"),
                    typeof(FragmentStackSupport.CountingFragment), null);
            mTabsAdapter.AddTab(mTabHost.NewTabSpec("contacts").SetIndicator("Contacts"),
                    typeof(LoaderCursorSupport.CursorLoaderListFragment), null);
            mTabsAdapter.AddTab(mTabHost.NewTabSpec("custom").SetIndicator("Custom"),
                    typeof(LoaderCustomSupport.AppListFragment), null);
            mTabsAdapter.AddTab(mTabHost.NewTabSpec("throttle").SetIndicator("Throttle"),
                    typeof(LoaderThrottleSupport.ThrottledLoaderListFragment), null);

            if (savedInstanceState != null) {
                mTabHost.SetCurrentTabByTag(savedInstanceState.GetString("tab"));
            }
        }

        protected override void OnSaveInstanceState(Bundle outState) {
            base.OnSaveInstanceState(outState);
            outState.PutString("tab", mTabHost.GetCurrentTabTag());
        }

        /**
         * This is a helper class that implements the management of tabs and all
         * details of connecting a ViewPager with associated TabHost.  It relies on a
         * trick.  Normally a tab host has a simple API for supplying a View or
         * Intent that each tab will show.  This is not sufficient for switching
         * between pages.  So instead we make the content part of the tab host
         * 0dp high (it is not shown) and the TabsAdapter supplies its own dummy
         * view to show as the tab content.  It listens to changes in tabs, and takes
         * care of switch to the correct paged in the ViewPager whenever the selected
         * tab changes.
         */
        public class TabsAdapter : FragmentPagerAdapter, TabHost.IOnTabChangeListener, ViewPager.IOnPageChangeListener {
            private readonly Context mContext;
            private readonly TabHost mTabHost;
            private readonly ViewPager mViewPager;
            private readonly ArrayList<TabInfo> mTabs = new ArrayList<TabInfo>();

            sealed class TabInfo {
                internal readonly string tag;
                internal readonly System.Type clss;
                internal readonly Bundle args;

                internal TabInfo(string _tag, System.Type _class, Bundle _args) {
                    tag = _tag;
                    clss = _class;
                    args = _args;
                }
            }

            class DummyTabFactory : TabHost.ITabContentFactory {
                private readonly Context mContext;

                public DummyTabFactory(Context context) {
                    mContext = context;
                }

                public View CreateTabContent(string tag) {
                    View v = new View(mContext);
                    v.SetMinimumWidth(0);
                    v.SetMinimumHeight(0);
                    return v;
                }
            }

            public TabsAdapter(FragmentActivity activity, TabHost tabHost, ViewPager pager) : base(activity.GetSupportFragmentManager())
            {
                mContext = activity;
                mTabHost = tabHost;
                mViewPager = pager;
                mTabHost.SetOnTabChangedListener(this);
                mViewPager.SetAdapter(this);
                mViewPager.SetOnPageChangeListener(this);
            }

            public void AddTab(TabHost.TabSpec tabSpec, System.Type clss, Bundle args) {
                tabSpec.SetContent(new DummyTabFactory(mContext));
                string tag = tabSpec.GetTag();

                TabInfo info = new TabInfo(tag, clss, args);
                mTabs.Add(info);
                mTabHost.AddTab(tabSpec);
                NotifyDataSetChanged();
            }

            public override int GetCount() {
                return mTabs.Size();
            }

            public override Fragment GetItem(int position) {
                TabInfo info = mTabs.Get(position);
                return Fragment.Instantiate(mContext, info.clss.FullName, info.args);
            }

            public void OnTabChanged(string tabId) {
                int position = mTabHost.GetCurrentTab();
                mViewPager.SetCurrentItem(position);
            }

            public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels) {
            }

            public void OnPageSelected(int position) {
                // Unfortunately when TabHost changes the current tab, it kindly
                // also takes care of putting focus on it when not in touch mode.
                // The jerk.
                // This hack tries to prevent this from pulling focus out of our
                // ViewPager.
                TabWidget widget = mTabHost.GetTabWidget();
                int oldFocusability = widget.GetDescendantFocusability();
                widget.SetDescendantFocusability(ViewGroup.FOCUS_BLOCK_DESCENDANTS);
                mTabHost.SetCurrentTab(position);
                widget.SetDescendantFocusability(oldFocusability);
            }

            public void OnPageScrollStateChanged(int state) {
            }
        }
    }
}