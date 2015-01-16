/*
 * Copyright (C) 2010 The Android Open Source Project
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
using com.example.android.supportv4;

using Android.Support.V4.App;

using Android.Content;
using Android.Content.Res;
using Android.Os;
using Android.Util;
using Android.View;
using Android.Widget;

using Dot42;
using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * Demonstration of using fragments to implement different activity layouts.
     * This sample provides a different layout (and activity flow) when run in
     * landscape.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_layout_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentLayoutSupport : FragmentActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            SetContentView(R.Layouts.fragment_layout_support);
        }


        /**
         * This is a secondary activity, to show what the user has selected
         * when the screen is not large enough to show it all in one activity.
         */
		[CustomView]
        [Activity(VisibleInLauncher = false)]
        public class DetailsActivity : FragmentActivity {

            protected override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);

                if (GetResources().GetConfiguration().Orientation
                        == Configuration.ORIENTATION_LANDSCAPE) {
                    // If the screen is now in landscape mode, we can show the
                    // dialog in-line with the list so we don't need this activity.
                    Finish();
                    return;
                }

                if (savedInstanceState == null) {
                    // During initial setup, plug in the details fragment.
                    DetailsFragment details = new DetailsFragment();
                    details.SetArguments(GetIntent().GetExtras());
                    GetSupportFragmentManager().BeginTransaction().Add(
                            global::Android.R.Id.Content, details).Commit();
                }
            }
        }


        /**
         * This is the "top-level" fragment, showing a list of items that the
         * user can pick.  Upon picking an item, it takes care of displaying the
         * data to the user as appropriate based on the currrent UI layout.
         */

		[CustomView]
        public class TitlesFragment : ListFragment {
            bool mDualPane;
            int mCurCheckPosition = 0;

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);

                // Populate list with our static array of titles.
                SetListAdapter(new ArrayAdapter<string>(GetActivity(),
                        R.Layouts.simple_list_item_checkable_1,
                        global::Android.R.Id.Text1, Shakespeare.TITLES));

                // Check to see if we have a frame in which to embed the details
                // fragment directly in the containing UI.
                View detailsFrame = GetActivity().FindViewById(R.Ids.details);
                mDualPane = detailsFrame != null && detailsFrame.GetVisibility() == View.VISIBLE;

                if (savedInstanceState != null) {
                    // Restore last state for checked position.
                    mCurCheckPosition = savedInstanceState.GetInt("curChoice", 0);
                }

                if (mDualPane) {
                    // In dual-pane mode, the list view highlights the selected item.
                    GetListView().SetChoiceMode(ListView.CHOICE_MODE_SINGLE);
                    // Make sure our UI is in the correct state.
                    ShowDetails(mCurCheckPosition);
                }
            }

            public override void OnSaveInstanceState(Bundle outState) {
                base.OnSaveInstanceState(outState);
                outState.PutInt("curChoice", mCurCheckPosition);
            }

            public override void OnListItemClick(ListView l, View v, int position, long id) {
                ShowDetails(position);
            }

            /**
             * Helper function to show the details of a selected item, either by
             * displaying a fragment in-place in the current UI, or starting a
             * whole new activity in which it is displayed.
             */
            void ShowDetails(int index) {
                mCurCheckPosition = index;

                if (mDualPane) {
                    // We can display everything in-place with fragments, so update
                    // the list to highlight the selected item and show the data.
                    GetListView().SetItemChecked(index, true);

                    // Check what fragment is currently shown, Replace if needed.
                    DetailsFragment details = (DetailsFragment)
                            GetFragmentManager().FindFragmentById(R.Ids.details);
                    if (details == null || details.GetShownIndex() != index) {
                        // Make new fragment to show this selection.
                        details = DetailsFragment.NewInstance(index);

                        // Execute a transaction, replacing any existing fragment
                        // with this one inside the frame.
                        FragmentTransaction ft = GetFragmentManager().BeginTransaction();
                        //if (index == 0) {
                            ft.Replace(R.Ids.details, details);
                        /*} else {
                            ft.Replace(R.Ids.a_item, details);
                        }*/
                        ft.SetTransition(FragmentTransaction.TRANSIT_FRAGMENT_FADE);
                        ft.Commit();
                    }

                } else {
                    // Otherwise we need to launch a new activity to display
                    // the dialog fragment with selected text.
                    Intent intent = new Intent();
                    intent.SetClass(GetActivity(), typeof(DetailsActivity));
                    intent.PutExtra("index", index);
                    StartActivity(intent);
                }
            }
        }


        /**
         * This is the secondary fragment, displaying the details of a particular
         * item.
         */

		[CustomView]
        public class DetailsFragment : Fragment {
            /**
             * Create a new instance of DetailsFragment, initialized to
             * show the text at 'index'.
             */
            public static DetailsFragment NewInstance(int index) {
                DetailsFragment f = new DetailsFragment();

                // Supply index input as an argument.
                Bundle args = new Bundle();
                args.PutInt("index", index);
                f.SetArguments(args);

                return f;
            }

            public int GetShownIndex() {
                return GetArguments().GetInt("index", 0);
            }

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                if (container == null) {
                    // We have different layouts, and in one of them this
                    // fragment's containing frame doesn't exist.  The fragment
                    // may still be created from its saved state, but there is
                    // no reason to try to create its view hierarchy because it
                    // won't be displayed.  Note this is not needed -- we could
                    // just run the code below, where we would create and return
                    // the view hierarchy; it would just never be used.
                    return null;
                }

                ScrollView scroller = new ScrollView(GetActivity());
                TextView text = new TextView(GetActivity());
                int pAdding = (int)TypedValue.ApplyDimension(TypedValue.COMPLEX_UNIT_DIP,
                        4, GetActivity().GetResources().GetDisplayMetrics());
                text.SetPadding(pAdding, pAdding, pAdding, pAdding);
                scroller.AddView(text);
                text.SetText(Shakespeare.DIALOGUE[GetShownIndex()]);
                return scroller;
            }
        }

    }
}