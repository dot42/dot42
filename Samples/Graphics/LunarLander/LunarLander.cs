/*
 * Copyright (C) 2007 The Android Open Source Project
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

using Android.App;
using Android.Os;
using Android.Util;
using Android.View;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("LunarLander", Icon = "@drawable/app_lunar_lander", Label = "@string/app_name")]

namespace LunarLander
{
    /// <summary>
    /// This is a simple LunarLander activity that houses a single LunarView. It
    /// demonstrates...
    /// <ul>
    /// <li>animating by calling invalidate() from draw()
    /// <li>loading and drawing resources
    /// <li>handling onPause() in an animation
    /// </ul>
    /// </summary>
    [Activity(Label = "LunarLander")]
    public class LunarLander : Activity
    {
        private const int MENU_EASY = 1;

        private const int MENU_HARD = 2;

        private const int MENU_MEDIUM = 3;

        private const int MENU_PAUSE = 4;

        private const int MENU_RESUME = 5;

        private const int MENU_START = 6;

        private const int MENU_STOP = 7;

        /// <summary>
        /// A handle to the thread that's actually running the animation. </summary>
        private LunarView.LunarThread mLunarThread;

        /// <summary>
        /// A handle to the View in which the game is running. </summary>
        private LunarView mLunarView;

        /// <summary>
        /// Invoked during init to give the Activity a chance to set up its Menu.
        /// </summary>
        /// <param name="menu"> the Menu to which entries may be added </param>
        /// <returns> true </returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);

            menu.Add(0, MENU_START, 0, R.Strings.menu_start);
            menu.Add(0, MENU_STOP, 0, R.Strings.menu_stop);
            menu.Add(0, MENU_PAUSE, 0, R.Strings.menu_pause);
            menu.Add(0, MENU_RESUME, 0, R.Strings.menu_resume);
            menu.Add(0, MENU_EASY, 0, R.Strings.menu_easy);
            menu.Add(0, MENU_MEDIUM, 0, R.Strings.menu_medium);
            menu.Add(0, MENU_HARD, 0, R.Strings.menu_hard);

            return true;
        }

        /// <summary>
        /// Invoked when the user selects an item from the Menu.
        /// </summary>
        /// <param name="item"> the Menu entry which was selected </param>
        /// <returns> true if the Menu item was legit (and we consumed it), false
        ///         otherwise </returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.GetItemId())
            {
                case MENU_START:
                    mLunarThread.DoStart();
                    return true;
                case MENU_STOP:
                    mLunarThread.SetState(LunarView.LunarThread.STATE_LOSE, GetText(R.Strings.message_stopped));
                    return true;
                case MENU_PAUSE:
                    mLunarThread.Pause();
                    return true;
                case MENU_RESUME:
                    mLunarThread.Unpause();
                    return true;
                case MENU_EASY:
                    mLunarThread.Difficulty = LunarView.LunarThread.DIFFICULTY_EASY;
                    return true;
                case MENU_MEDIUM:
                    mLunarThread.Difficulty = LunarView.LunarThread.DIFFICULTY_MEDIUM;
                    return true;
                case MENU_HARD:
                    mLunarThread.Difficulty = LunarView.LunarThread.DIFFICULTY_HARD;
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Invoked when the Activity is created.
        /// </summary>
        /// <param name="savedInstanceState"> a Bundle containing state saved from a previous
        ///        execution, or null if this is a new execution </param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // tell system to use the layout defined in our XML file
            SetContentView(R.Layouts.lunar_layout);

            // get handles to the LunarView from XML, and its LunarThread
            mLunarView = (LunarView) FindViewById(R.Ids.lunar);
            mLunarThread = mLunarView.Thread;

            // give the LunarView a handle to the TextView used for messages
            mLunarView.TextView = (TextView) FindViewById(R.Ids.text);

            if (savedInstanceState == null)
            {
                // we were just launched: set up a new game
                mLunarThread.state = LunarView.LunarThread.STATE_READY;
                Log.W(GetType().Name, "SIS is null");
            }
            else
            {
                // we are being restored: resume a previous game
                mLunarThread.RestoreState(savedInstanceState);
                Log.W(GetType().Name, "SIS is nonnull");
            }
        }

        /// <summary>
        /// Invoked when the Activity loses user focus.
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
            mLunarView.Thread.Pause(); // pause game when Activity pauses
        }

        /// <summary>
        /// Notification that something is about to happen, to give the Activity a
        /// chance to save state.
        /// </summary>
        /// <param name="outState"> a Bundle into which this Activity should save its state </param>
        protected override void OnSaveInstanceState(Bundle outState)
        {
            // just have the View's thread save its state into our Bundle
            base.OnSaveInstanceState(outState);
            mLunarThread.SaveState(outState);
            Log.W(GetType().Name, "SIS called");
        }
    }
}