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

using Android.Os;
using Android.View;
using Android.Widget;

using System.Threading;
using Java.Lang;

using Dot42.Manifest;

namespace com.example.android.supportv4.app
{
    /**
     * This example shows how you can use a Fragment to easily propagate state
     * (such as threads) across activity instances when an activity needs to be
     * restarted due to, for example, a configuration change.  This is a lot
     * easier than using the raw Activity.onRetainNonConfiguratinInstance() API.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/fragment_retain_instance_support")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class FragmentRetainInstanceSupport : FragmentActivity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // First time init, create the UI.
            if (savedInstanceState == null) {
                GetSupportFragmentManager().BeginTransaction().Add(global::Android.R.Id.Content,
                        new UiFragment()).Commit();
            }
        }

        /**
         * This is a fragment showing UI that will be updated from work done
         * in the retained fragment.
         */
        public class UiFragment : Fragment {
            RetainedFragment mWorkFragment;

            public override View OnCreateView(LayoutInflater Inflater, ViewGroup container,
                    Bundle savedInstanceState) {
                View v = Inflater.Inflate(R.Layouts.fragment_retain_instance, container, false);

                // Watch for button clicks.
                Button button = (Button)v.FindViewById(R.Ids.restart);
                button.Click += (o,a) => mWorkFragment.Restart();
                
                return v;
            }

            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);

                FragmentManager fm = GetFragmentManager();

                // Check to see if we have retained the worker fragment.
                mWorkFragment = (RetainedFragment)fm.FindFragmentByTag("work");

                // If not retained (or first time running), we need to create it.
                if (mWorkFragment == null) {
                    mWorkFragment = new RetainedFragment();
                    // Tell it who it is working with.
                    mWorkFragment.SetTargetFragment(this, 0);
                    fm.BeginTransaction().Add(mWorkFragment, "work").Commit();
                }
            }

        }

        /**
         * This is the Fragment implementation that will be retained across
         * activity instances.  It represents some ongoing work, here a thread
         * we have that sits around incrementing a progress indicator.
         */
        public class RetainedFragment : Fragment {
            ProgressBar mProgressBar;
            int mPosition;
            bool mReady = false;
            bool mQuiting = false;

            class MyThread : Thread
            {
                private RetainedFragment retainedFragment;

                public MyThread(RetainedFragment retainedFragment)
                {
                    this.retainedFragment = retainedFragment;
                }

                public override void Run() {
                    // We'll figure the real value out later.
                    int max = 10000;

                    // This thread runs almost forever.
                    while (true) {

                        // Update our shared state with the UI.
                        lock (retainedFragment) {
                            // Our thread is stopped if the UI is not ready
                            // or it has completed its work.
                            while (!retainedFragment.mReady || retainedFragment.mPosition >= max)
                            {
                                if (retainedFragment.mQuiting)
                                {
                                    return;
                                }
                                try {
                                    retainedFragment.JavaWait();
                                } catch (InterruptedException) {
                                }
                            }

                            // Now update the progress.  Note it is usingant that
                            // we touch the progress bar with the lock held, so it
                            // doesn't disappear on us.
                            retainedFragment.mPosition++;
                            max = retainedFragment.mProgressBar.GetMax();
                            retainedFragment.mProgressBar.SetProgress(retainedFragment.mPosition);
                        }

                        // Normally we would be doing some work, but put a kludge
                        // here to pretend like we are.
                        lock (retainedFragment) {
                            try {
                                retainedFragment.JavaWait(50);
                            } catch (InterruptedException) {
                            }
                        }
                    }
                }
            }

            public RetainedFragment()
            {
                mThread = new MyThread(this);
            }

            /**
             * This is the thread that will do our work.  It sits in a loop running
             * the progress up until it has reached the top, then stops and waits.
             */
            readonly Thread mThread;

            /**
             * Fragment initialization.  We way we want to be retained and
             * start our thread.
             */
            public override void OnCreate(Bundle savedInstanceState) {
                base.OnCreate(savedInstanceState);

                // Tell the framework to try to keep this fragment around
                // during a configuration change.
                SetRetainInstance(true);

                // Start up the worker thread.
                mThread.Start();
            }

            /**
             * This is called when the Fragment's Activity is ready to go, after
             * its content view has been installed; it is called both after
             * the initial fragment creation and after the fragment is re-attached
             * to a new activity.
             */
            public override void OnActivityCreated(Bundle savedInstanceState) {
                base.OnActivityCreated(savedInstanceState);

                // Retrieve the progress bar from the target's view hierarchy.
                mProgressBar = (ProgressBar)GetTargetFragment().GetView().FindViewById(
                        R.Ids.progress_horizontal);

                // We are ready for our thread to go.
                lock (mThread) {
                    mReady = true;
                    mThread.Notify();
                }
            }

            /**
             * This is called when the fragment is going away.  It is NOT called
             * when the fragment is being propagated between activity instances.
             */
            public override void OnDestroy() {
                // Make the thread go away.
                lock (mThread) {
                    mReady = false;
                    mQuiting = true;
                    mThread.Notify();
                }

                base.OnDestroy();
            }

            /**
             * This is called right before the fragment is detached from its
             * current activity instance.
             */
            public override void OnDetach() {
                // This fragment is being detached from its activity.  We need
                // to make sure its thread is not going to touch any activity
                // state after returning from this function.
                lock (mThread) {
                    mProgressBar = null;
                    mReady = false;
                    mThread.Notify();
                }

                base.OnDetach();
            }

            /**
             * API for our UI to restart the progress thread.
             */
            public void Restart() {
                lock (mThread) {
                    mPosition = 0;
                    mThread.Notify();
                }
            }
        }
    }
}