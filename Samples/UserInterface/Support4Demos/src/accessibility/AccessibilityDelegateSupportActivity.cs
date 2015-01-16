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

using Android.App;
using Android.Content;
using Android.Os;
using Android.Support.V4.View;
using Android.Support.V4.View.Accessibility;
using Android.Util;
using Android.View;
using Android.View.Accessibility;

using Dot42;
using Dot42.Manifest;

namespace com.example.android.supportv4.accessibility
{
    /**
     * This class demonstrates how to use the support library to register
     * a View.AccessibilityDelegate that customizes the accessibility
     * behavior of a View. Aiming to maximize simplicity this example
     * tweaks the text reported to accessibility services but using
     * these APIs a client can inject any accessibility functionality into
     * a View.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/accessibility_delegate_title")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class AccessibilityDelegateSupportActivity : Activity {

        /**
         * {@inheritDoc}
         */
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.accessibility_delegate);
        }

        /**
         * This class represents a View that is customized via an AccessibilityDelegate
         * as opposed to inheritance. An accessibility delegate can be used for Adding
         * accessibility to custom Views, i.e. ones that extend classes from Android.view,
         * in a backwards compatible fashion. Note that overriding a method whose return
         * type or arguments are not part of a target platform APIs makes your application
         * not backwards compatible with that platform version.
         */
        [CustomView]
        public class AccessibilityDelegateSupportView : View {

            private class MyAccessibilityDelegateCompat : AccessibilityDelegateCompat
            {
                //TODO: is host.GetContect what we want, or should be create an Additional field in this class?

                public override void OnPopulateAccessibilityEvent(View host, AccessibilityEvent @event)
                {
                    base.OnPopulateAccessibilityEvent(host, @event);
                    // Note that View.onPopulateAccessibilityEvent was introduced in
                    // ICS and we would like to tweak a bit the text that is reported to
                    // accessibility services via the AccessibilityEvent.
                    @event.GetText().Add(host.GetContext().GetString(
                            R.Strings.accessibility_delegate_custom_text_added));
                }

                public override void OnInitializeAccessibilityNodeInfo(View host,
                        AccessibilityNodeInfoCompat info)
                {
                    base.OnInitializeAccessibilityNodeInfo(host, info);
                    // Note that View.onInitializeAccessibilityNodeInfo was introduced in
                    // ICS and we would like to tweak a bit the text that is reported to
                    // accessibility services via the AccessibilityNodeInfo.
                    info.SetText(host.GetContext().GetString(
                            R.Strings.accessibility_delegate_custom_text_added));
                }
            }

            public AccessibilityDelegateSupportView(Context context, IAttributeSet attrs):
                base(context, attrs)
            {
                InstallAccessibilityDelegate();
            }

            private void InstallAccessibilityDelegate() {
                // The accessibility delegate enables customizing accessibility behavior
                // via composition as opposed as inheritance. The main benefit is that
                // one can write a backwards compatible application by setting the delegate
                // only if the API level is high enough i.e. the delegate is part of the APIs.
                // The easiest way to achieve that is by using the support library which
                // takes the burden of checking API version and knowing which API version
                // introduced the delegate off the developer.
                ViewCompat.SetAccessibilityDelegate(this, new MyAccessibilityDelegateCompat());
            }
        }
    }
}
