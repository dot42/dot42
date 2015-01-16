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
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.View;
using Android.Widget;

using Dot42.Manifest;

namespace com.example.android.supportv4.content
{
    /**
     * Demonstrates the use of a LocalBroadcastManager to easily communicate
     * data from a service to any other interested code.
     */
    [Activity(VisibleInLauncher = false, Label = "@string/local_service_broadcaster")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "com.example.android.supportv4.SUPPORT4_SAMPLE_CODE" })]
    public class LocalServiceBroadcaster : Activity {
        const string ACTION_STARTED = "com.example.android.supportv4.STARTED";
        const string ACTION_UPDATE = "com.example.android.supportv4.UPDATE";
        const string ACTION_STOPPED = "com.example.android.supportv4.STOPPED";

        LocalBroadcastManager mLocalBroadcastManager;
        BroadcastReceiver mReceiver;

        class MyBroadcastReceiver : BroadcastReceiver
        {
             readonly TextView callbackData;

             public MyBroadcastReceiver (TextView callbackData)
	        {
                 this.callbackData = callbackData;
	        }

             public override void OnReceive(Context context, Intent intent) {
                    if (intent.GetAction().Equals(ACTION_STARTED)) {
                        callbackData.SetText("STARTED");
                    } else if (intent.GetAction().Equals(ACTION_UPDATE)) {
                        callbackData.SetText("Got update: " + intent.GetIntExtra("value", 0));
                    } else if (intent.GetAction().Equals(ACTION_STOPPED)) {
                        callbackData.SetText("STOPPED");
					}
                }
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            SetContentView(R.Layouts.local_service_broadcaster);

            // This is where we print the data we get back.
            TextView callbackData = (TextView)FindViewById(R.Ids.callback);

            // Put in some initial text.
            callbackData.SetText("No broadcast received yet");

            // We use this to send broadcasts within our local process.
            mLocalBroadcastManager = LocalBroadcastManager.GetInstance(this);

            // We are going to watch for interesting local broadcasts.
            IntentFilter filter = new IntentFilter();
            filter.AddAction(ACTION_STARTED);
            filter.AddAction(ACTION_UPDATE);
            filter.AddAction(ACTION_STOPPED);
            mReceiver = new MyBroadcastReceiver(callbackData);
            
            mLocalBroadcastManager.RegisterReceiver(mReceiver, filter);

            // Watch for button clicks.
            Button button = (Button)FindViewById(R.Ids.start);
            button.Click += (o,e) => StartService(new Intent(this, typeof(LocalService)));
            button = (Button)FindViewById(R.Ids.stop);
            button.Click += (o,e) => StopService(new Intent(this, typeof(LocalService)));
        }

        protected override void OnDestroy() 
        {
            base.OnDestroy();
            mLocalBroadcastManager.UnregisterReceiver(mReceiver);
        }

        [Service(StopWithTask=true)]
        public class LocalService : Service {
            LocalBroadcastManager mLocalBroadcastManager;
            int mCurUpdate;

            class MyHandler :  Handler 
            {
                readonly LocalService localService;

                public MyHandler(LocalService localService)
	            {
                    this.localService = localService;
	            }

                public override void HandleMessage(Message msg) {
                    switch (msg.What) {
                        case MSG_UPDATE: {
                            localService.mCurUpdate++;
                            Intent intent = new Intent(ACTION_UPDATE);
                            intent.PutExtra("value", localService.mCurUpdate);
                            localService.mLocalBroadcastManager.SendBroadcast(intent);
                            Message nmsg = localService.mHandler.ObtainMessage(MSG_UPDATE);
                            localService.mHandler.SendMessageDelayed(nmsg, 1000);
                        } break;
                        default:
                            base.HandleMessage(msg);
                            break;
                    }
                }
            }

            const int MSG_UPDATE = 1;

            public LocalService()
            {
                mHandler = new MyHandler(this);
            }

            Handler mHandler;

            public override void OnCreate() {
                base.OnCreate();
                mLocalBroadcastManager = LocalBroadcastManager.GetInstance(this);
            }

            public override int OnStartCommand(Intent intent, int flags, int startId) {
                // Tell any local interested parties about the start.
                mLocalBroadcastManager.SendBroadcast(new Intent(ACTION_STARTED));

                // Prepare to do update reports.
                mHandler.RemoveMessages(MSG_UPDATE);
                Message msg = mHandler.ObtainMessage(MSG_UPDATE);
                mHandler.SendMessageDelayed(msg, 1000);
                return ServiceCompat.START_STICKY;
            }

            public override void OnDestroy() {
                base.OnDestroy();

                // Tell any local interested parties about the stop.
                mLocalBroadcastManager.SendBroadcast(new Intent(ACTION_STOPPED));

                // Stop doing updates.
                mHandler.RemoveMessages(MSG_UPDATE);
            }

            public override IBinder OnBind(Intent intent)
            {
 	            return null;
            }
        }
    }
}