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

using Android.Content.Res;
using Android.Os;
using Android.Util;
using Android.View;
using Android.Widget;

using Java.Lang;

using Dot42;
using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstrates a fragment that can be configured through both Bundle arguments
     * and layout attributes.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_arguments_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentArgumentsSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(R.Layouts.fragment_arguments_support);

            if (savedInstanceState == null) {
                // First-time init; create fragment to embed in activity.
                FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
                Fragment newFragment = MyFragment.NewInstance("From Arguments");
                ft.Add(R.Ids.created, newFragment);
                ft.Commit();
            }
        }


		[CustomView]
        public class MyFragment : Fragment {
            ICharSequence mLabel;

            /**
             * Create a new instance of MyFragment that will be initialized
             * with the given arguments.
             */
            internal static MyFragment NewInstance(ICharSequence label) {
                MyFragment f = new MyFragment();
                Bundle b = new Bundle();
                b.PutCharSequence("label", label);
                f.SetArguments(b);
                return f;
            }

            /**
             * Parse attributes during inflation from a view hierarchy into the
             * arguments we handle.
             */
            public override void OnInflate(global::Android.App.Activity activity, IAttributeSet attrs,
                    Bundle savedInstanceState) {
                base.OnInflate(activity, attrs, savedInstanceState);

                TypedArray a = activity.ObtainStyledAttributes(attrs, R.Styleables.FragmentArguments.AllIds);
                mLabel = a.GetText(R.Styleables.FragmentArguments.label & 0xFFFF);
                a.Recycle();
            }

            /**
             * During creation, if arguments have been supplied to the fragment
             * then Parse those out.
             */
            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);

                Bundle args = GetArguments();
                if (args != null) {
                    ICharSequence label = args.GetCharSequence("label");
                    if (label != null) {
                        mLabel = label;
                    }
                }
            }

            /**
             * Create the view for this fragment, using the arguments given to it.
             */
            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.hello_world, container, false);
                View tv = v.FindViewById(R.Ids.text);
                ((TextView)tv).SetText(mLabel != null ? mLabel : "(no label)");
                tv.SetBackgroundDrawable(GetResources().GetDrawable(global::Android.R.Drawable.Gallery_thumb));
                return v;
            }
        }

    }
}