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
using System.Text;

using Android.Accessibilityservice;
using Android.App;
using Android.Content.Pm;
using Android.Os;
using Android.Support.V4.Accessibilityservice;
using Android.Support.V4.View.Accessibility;
using Android.View.Accessibility;
using Android.Widget;

using Support4Demos;

using Java.Util;

using Dot42.Manifest;

namespace com.example.android.supportv4.accessibility
{
    /**
     * <p>
     * This class demonstrates how to use the support library to register
     * an AccessibilityManager.AccessibilityStateChangeListener introduced
     * in ICS to watch changes to the global accessibility state on the
     * device in a backwards compatible manner.
     * </p>
     * <p>
     * This class also demonstrates how to use the support library to query
     * information about enabled accessibility services via APIs introduced
     * in ICS in a backwards compatible manner.
     * </p>
     */
    [Activity(VisibleInLauncher = false, Label = "@string/accessibility_manager_title")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class AccessibilityManagerSupportActivity : Activity {

        private class MyAccessibilityStateChangeListenerCompat : AccessibilityManagerCompat.AccessibilityStateChangeListenerCompat
        {
            public AccessibilityManagerSupportActivity accessibilityManagerSupportActivity;

            public MyAccessibilityStateChangeListenerCompat(AccessibilityManagerSupportActivity accessibilityManagerSupportActivity)
            {
                this.accessibilityManagerSupportActivity = accessibilityManagerSupportActivity;
            }

            public override void OnAccessibilityStateChanged(bool enabled) 
            {
                Toast.MakeText(accessibilityManagerSupportActivity,
                            accessibilityManagerSupportActivity.GetString(R.Strings.accessibility_manager_accessibility_state, enabled),
                            Toast.LENGTH_SHORT).Show();
            }
        }

        /** Handle to the accessibility manager service. */
        private AccessibilityManager mAccessibilityManager;

        /** Handle to the View showing accessibility services summary */
        private TextView mAccessibilityStateView;

        /**
         * {@inheritDoc}
         */
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.accessibility_manager);
            mAccessibilityManager = (AccessibilityManager) GetSystemService(
                    Service.ACCESSIBILITY_SERVICE);
            mAccessibilityStateView = (TextView) FindViewById(R.Ids.accessibility_state);
            RegisterAccessibilityStateChangeListener();
        }

        /**
         * {@inheritDoc}
         */
        protected override void OnResume()
        {
            base.OnResume();
            UpdateAccessibilityStateView();
        }

        /**
         * Registers an AccessibilityStateChangeListener that show a Toast
         * when the global accessibility state on the device changes.
         */
        private void RegisterAccessibilityStateChangeListener() {
            // The AccessibilityStateChange listener APIs were Added in ICS. Therefore to be
            // backwards compatible we use the APIs in the support library. Note that if the
            // platform API version is lower and the called API is not available no listener
            // is Added and you will not receive a call of onAccessibilityStateChanged.
            AccessibilityManagerCompat.AddAccessibilityStateChangeListener(mAccessibilityManager,
                    new MyAccessibilityStateChangeListenerCompat(this));
        }

        /**
         * Updates the content of a TextView with description of the enabled
         * accessibility services.
         */
        private void UpdateAccessibilityStateView() {
            // The API for getting the enabled accessibility services based on feedback
            // type was Added in ICS. Therefore to be backwards compatible we use the
            // APIs in the support library. Note that if the platform API version is lower
            // and the called API is not available an empty list of services is returned.
            IList<AccessibilityServiceInfo> enabledServices =
                AccessibilityManagerCompat.GetEnabledAccessibilityServiceList(mAccessibilityManager,
                        AccessibilityServiceInfo.FEEDBACK_SPOKEN);
            if (!enabledServices.IsEmpty()) {
                StringBuilder builder = new StringBuilder();
                int enabledServiceCount = enabledServices.Size();
                for (int i = 0; i < enabledServiceCount; i++) {
                    AccessibilityServiceInfo service = enabledServices.Get(i);
                    // Some new APIs were Added in ICS for getting more information about
                    // an accessibility service. Again accessed them via the support library.
                    ResolveInfo resolveInfo = AccessibilityServiceInfoCompat.GetResolveInfo(service);
                    string serviceDescription = GetString(
                            R.Strings.accessibility_manager_enabled_service,
                            resolveInfo.LoadLabel(GetPackageManager()),
                            AccessibilityServiceInfoCompat.FeedbackTypeToString(service.FeedbackType),
                            AccessibilityServiceInfoCompat.GetDescription(service),
                            AccessibilityServiceInfoCompat.GetSettingsActivityName(service));
                    builder.Append(serviceDescription);
                }
                mAccessibilityStateView.SetText(builder);
            } else {
                // Either no services or the platform API version is not high enough.
                mAccessibilityStateView.SetText(GetString(
                        R.Strings.accessibility_manager_no_enabled_services));
            }
        }
    }
}