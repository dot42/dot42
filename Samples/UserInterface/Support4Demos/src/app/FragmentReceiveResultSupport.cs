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

using com.example.android.supportv4.app;
using Support4Demos;

using Android.Support.V4.App;

using Android.Content;
using Android.Os;
using Android.Text;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_receive_result_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentReceiveResultSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            FrameLayout.LayoutParams lp = new FrameLayout.LayoutParams(
                    ViewGroup.LayoutParams.FILL_PARENT,
                    ViewGroup.LayoutParams.FILL_PARENT);
            FrameLayout frame = new FrameLayout(this);
            frame.SetId(R.Ids.simple_fragment);
            SetContentView(frame, lp);

            if (savedInstanceState == null) {
                // Do first time initialization -- Add fragment.
                Fragment newFragment = new ReceiveResultFragment();
                FragmentTransaction ft = GetSupportFragmentManager().BeginTransaction();
                ft.Add(R.Ids.simple_fragment, newFragment).Commit();
            }
        }

        public class ReceiveResultFragment : Fragment {
            // Definition of the one requestCode we use for receiving resuls.
            private const int GET_CODE = 1;

            private TextView mResults;

            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);
            }

            public override void OnSaveInstanceState(Bundle outState) {
                base.OnSaveInstanceState(outState);
            }

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.receive_result, container, false);

                // Retrieve the TextView widget that will display results.
                mResults = (TextView)v.FindViewById(R.Ids.results);

                // This allows us to later extend the text buffer.
                mResults.SetText(mResults.GetText(), TextView.BufferType.EDITABLE);

                // Watch for button clicks.
                Button getButton = (Button)v.FindViewById(R.Ids.get);
                getButton.Click += (o, a) =>
                {
                    // Start the activity whose result we want to retrieve.  The
                    // result will come back with request code GET_CODE.
                    Intent intent = new Intent(GetActivity(), typeof(SendResult));
                    StartActivityForResult(intent, GET_CODE);
                };

                return v;
            }

            /**
             * This method is called when the sending activity has Finished, with the
             * result it supplied.
             */
            public override void OnActivityResult(int requestCode, int resultCode, Intent data) {
                // You can use the requestCode to select between multiple child
                // activities you may have started.  Here there is only one thing
                // we launch.
                if (requestCode == GET_CODE) {

                    // We will be Adding to our text.
                    IEditable text = (IEditable)mResults.GetText();

                    // This is a standard resultCode that is sent back if the
                    // activity doesn't supply an explicit result.  It will also
                    // be returned if the activity failed to launch.
                    if (resultCode == RESULT_CANCELED) {
                        text.Append("(cancelled)");

                    // Our protocol with the sending activity is that it will send
                    // text in 'data' as its result.
                    } else {
                        text.Append("(okay ");
                        text.Append(int.ToString(resultCode));
                        text.Append(") ");
                        if (data != null) {
                            text.Append(data.GetAction());
                        }
                    }

                    text.Append("\n");
                }
            }
        }
    }
}