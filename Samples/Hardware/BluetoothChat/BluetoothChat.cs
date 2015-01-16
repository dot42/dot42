using System.Runtime.CompilerServices;
using System.Text;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Os;
using Android.Util;
using Android.View;
using Android.View.Inputmethod;
using Android.Widget;
using Dot42.Manifest;
using Java.Lang;

/*
 * Copyright (C) 2009 The Android Open Source Project
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

[assembly: Application("BluetoothChat", Label = "@string/app_name", Icon = "app_icon")]

[assembly: UsesPermission(Android.Manifest.Permission.BLUETOOTH_ADMIN)]
[assembly: UsesPermission(Android.Manifest.Permission.BLUETOOTH)]

namespace BluetoothChat
{
    /// <summary>
    /// This is the main Activity that displays the current chat session.
    /// </summary>
    [Activity(Label = "@string/app_name", ConfigChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class BluetoothChat : Activity
    {
        // Debugging
        private const string TAG = "BluetoothChat";
        private const bool D = true;

        // Message types sent from the BluetoothChatService Handler
        public const int MESSAGE_STATE_CHANGE = 1;
        public const int MESSAGE_READ = 2;
        public const int MESSAGE_WRITE = 3;
        public const int MESSAGE_DEVICE_NAME = 4;
        public const int MESSAGE_TOAST = 5;

        // Key names received from the BluetoothChatService Handler
        public const string DEVICE_NAME = "device_name";
        public const string TOAST = "toast";

        // Intent request codes
        private const int REQUEST_CONNECT_DEVICE_SECURE = 1;
        private const int REQUEST_CONNECT_DEVICE_INSECURE = 2;
        private const int REQUEST_ENABLE_BT = 3;

        // Layout Views
        private ListView mConversationView;
        private EditText mOutEditText;
        private Button mSendButton;

        // Name of the connected device
        private string mConnectedDeviceName;
        // Array adapter for the conversation thread
        private ArrayAdapter<string> mConversationArrayAdapter;
        // String buffer for outgoing messages
        private StringBuilder mOutStringBuffer;
        // Local Bluetooth adapter
        private BluetoothAdapter mBluetoothAdapter;
        // Member object for the chat services
        private BluetoothChatService mChatService;

        private readonly MyHandler mHandler;

        public BluetoothChat()
        {
            mHandler = new MyHandler(this);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (D)
            {
                Log.E(TAG, "+++ ON CREATE +++");
            }

            // Set up the window layout
            SetContentView(R.Layouts.main);

            // Get local Bluetooth adapter
            mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (mBluetoothAdapter == null)
            {
                Toast.MakeText(this, "Bluetooth is not available", Toast.LENGTH_LONG).Show();
                Finish();
                return;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (D)
            {
                Log.E(TAG, "++ ON START ++");
            }

            // If BT is not on, request that it be enabled.
            // setupChat() will then be called during onActivityResult
            if (!mBluetoothAdapter.IsEnabled())
            {
                Intent enableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
                StartActivityForResult(enableIntent, REQUEST_ENABLE_BT);
                // Otherwise, setup the chat session
            }
            else
            {
                if (mChatService == null)
                {
                    SetupChat();
                }
            }
        }

        protected override void OnResume()
        {
            lock (this)
            {
                base.OnResume();
                if (D)
                {
                    Log.E(TAG, "+ ON RESUME +");
                }

                // Performing this check in onResume() covers the case in which BT was
                // not enabled during onStart(), so we were paused to enable it...
                // onResume() will be called when ACTION_REQUEST_ENABLE activity returns.
                if (mChatService != null)
                {
                    // Only if the state is STATE_NONE, do we know that we haven't started already
                    if (mChatService.State == BluetoothChatService.STATE_NONE)
                    {
                        // Start the Bluetooth chat services
                        mChatService.Start();
                    }
                }
            }
        }

        private void SetupChat()
        {
            Log.D(TAG, "setupChat()");

            // Initialize the array adapter for the conversation thread
            mConversationArrayAdapter = new ArrayAdapter<string>(this, R.Layouts.message);
            mConversationView = (ListView)FindViewById(R.Ids.@in);
            mConversationView.SetAdapter(mConversationArrayAdapter);

            // Initialize the compose field with a listener for the return key
            mOutEditText = (EditText)FindViewById(R.Ids.edit_text_out);
            mOutEditText.EditorAction += (s, x) => {
                // If the action is a key-up event on the return key, send the message
                if (x.ActionId == EditorInfo.IME_NULL && x.Event.GetAction() == KeyEvent.ACTION_UP)
                {
                    var message = ((TextView)s).GetText().ToString();
                    SendMessage(message);
                }
                if (D)
                    Log.I(TAG, "END onEditorAction");
                x.IsHandled = true;
            };

            // Initialize the send button with a listener that for click events
            mSendButton = (Button)FindViewById(R.Ids.button_send);
            mSendButton.Click += (s, x) => {
                // Send a message using content of the edit text widget
                var view = (TextView)FindViewById(R.Ids.edit_text_out);
                var message = view.GetText().ToString();
                SendMessage(message);
            };

            // Initialize the BluetoothChatService to perform bluetooth connections
            mChatService = new BluetoothChatService(this, mHandler);

            // Initialize the buffer for outgoing messages
            mOutStringBuffer = new StringBuilder("");
        }

        protected override void OnPause()
        {
            lock (this)
            {
                base.OnPause();
                if (D)
                {
                    Log.E(TAG, "- ON PAUSE -");
                }
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (D)
            {
                Log.E(TAG, "-- ON STOP --");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Stop the Bluetooth chat services
            if (mChatService != null)
            {
                mChatService.Stop();
            }
            if (D)
            {
                Log.E(TAG, "--- ON DESTROY ---");
            }
        }

        private void EnsureDiscoverable()
        {
            if (D)
            {
                Log.D(TAG, "ensure discoverable");
            }
            if (mBluetoothAdapter.GetScanMode() != BluetoothAdapter.SCAN_MODE_CONNECTABLE_DISCOVERABLE)
            {
                var discoverableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_DISCOVERABLE);
                discoverableIntent.PutExtra(BluetoothAdapter.EXTRA_DISCOVERABLE_DURATION, 300);
                StartActivity(discoverableIntent);
            }
        }

        /// <summary>
        /// Sends a message. </summary>
        /// <param name="message">  A string of text to send. </param>
        private void SendMessage(string message)
        {
            // Check that we're actually connected before trying anything
            if (mChatService.State != BluetoothChatService.STATE_CONNECTED)
            {
                Toast.MakeText(this, R.Strings.not_connected, Toast.LENGTH_SHORT).Show();
                return;
            }

            // Check that there's actually something to send
            if (message.Length > 0)
            {
                // Get the message bytes and tell the BluetoothChatService to write
                sbyte[] send = message.JavaGetBytes();
                mChatService.Write(send);

                // Reset out string buffer to zero and clear the edit text field
                mOutStringBuffer.Length = 0;
                mOutEditText.SetText(mOutStringBuffer);
            }
        }

        private void SetStatus(int value)
        {
            var actionBar = GetActionBar();
            actionBar.SetSubtitle(value);
        }

        private void SetStatus(ICharSequence value)
        {
            var actionBar = GetActionBar();
            actionBar.SetSubtitle(value);
        }

        // The Handler that gets information back from the BluetoothChatService
        private class MyHandler : Handler
        {
            private readonly BluetoothChat chat;

            public MyHandler(BluetoothChat chat)
            {
                this.chat = chat;
            }

            public override void HandleMessage(Message msg)
            {
                switch (msg.What)
                {
                    case MESSAGE_STATE_CHANGE:
                        if (D)
                            Log.I(TAG, "MESSAGE_STATE_CHANGE: " + msg.Arg1);
                        switch (msg.Arg1)
                        {
                            case BluetoothChatService.STATE_CONNECTED:
                                chat.SetStatus(chat.GetString(R.Strings.title_connected_to, chat.mConnectedDeviceName));
                                chat.mConversationArrayAdapter.Clear();
                                break;
                            case BluetoothChatService.STATE_CONNECTING:
                                chat.SetStatus(R.Strings.title_connecting);
                                break;
                            case BluetoothChatService.STATE_LISTEN:
                            case BluetoothChatService.STATE_NONE:
                                chat.SetStatus(R.Strings.title_not_connected);
                                break;
                        }
                        break;
                    case MESSAGE_WRITE:
                        byte[] writeBuf = (byte[])msg.Obj;
                        // construct a string from the buffer
                        var writeMessage = new string(writeBuf);
                        chat.mConversationArrayAdapter.Add("Me:  " + writeMessage);
                        break;
                    case MESSAGE_READ:
                        byte[] readBuf = (byte[])msg.Obj;
                        // construct a string from the valid bytes in the buffer
                        var readMessage = new string(readBuf, 0, msg.Arg1);
                        chat.mConversationArrayAdapter.Add(chat.mConnectedDeviceName + ":  " + readMessage);
                        break;
                    case MESSAGE_DEVICE_NAME:
                        // save the connected device's name
                        chat.mConnectedDeviceName = msg.GetData().GetString(DEVICE_NAME);
                        Toast.MakeText(chat.GetApplicationContext(), "Connected to " + chat.mConnectedDeviceName, Toast.LENGTH_SHORT).Show();
                        break;
                    case MESSAGE_TOAST:
                        Toast.MakeText(chat.GetApplicationContext(), msg.GetData().GetString(TOAST), Toast.LENGTH_SHORT).Show();
                        break;
                }
            }
        }

        protected override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (D)
            {
                Log.D(TAG, "onActivityResult " + resultCode);
            }
            switch (requestCode)
            {
                case REQUEST_CONNECT_DEVICE_SECURE:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Activity.RESULT_OK)
                    {
                        ConnectDevice(data, true);
                    }
                    break;
                case REQUEST_CONNECT_DEVICE_INSECURE:
                    // When DeviceListActivity returns with a device to connect
                    if (resultCode == Activity.RESULT_OK)
                    {
                        ConnectDevice(data, false);
                    }
                    break;
                case REQUEST_ENABLE_BT:
                    // When the request to enable Bluetooth returns
                    if (resultCode == Activity.RESULT_OK)
                    {
                        // Bluetooth is now enabled, so set up a chat session
                        SetupChat();
                    }
                    else
                    {
                        // User did not enable Bluetooth or an error occurred
                        Log.D(TAG, "BT not enabled");
                        Toast.MakeText(this, R.Strings.bt_not_enabled_leaving, Toast.LENGTH_SHORT).Show();
                        Finish();
                    }
                    break;
            }
        }

        private void ConnectDevice(Intent data, bool secure)
        {
            // Get the device MAC address
            string address = data.GetExtras().GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
            // Get the BluetoothDevice object
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            // Attempt to connect to the device
            mChatService.Connect(device, secure);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = GetMenuInflater();
            inflater.Inflate(R.Menus.option_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Intent serverIntent = null;
            switch (item.GetItemId())
            {
                case R.Ids.secure_connect_scan:
                    // Launch the DeviceListActivity to see devices and do scan
                    serverIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(serverIntent, REQUEST_CONNECT_DEVICE_SECURE);
                    return true;
                case R.Ids.insecure_connect_scan:
                    // Launch the DeviceListActivity to see devices and do scan
                    serverIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(serverIntent, REQUEST_CONNECT_DEVICE_INSECURE);
                    return true;
                case R.Ids.discoverable:
                    // Ensure this device is discoverable by others
                    EnsureDiscoverable();
                    return true;
            }
            return false;
        }

    }

}