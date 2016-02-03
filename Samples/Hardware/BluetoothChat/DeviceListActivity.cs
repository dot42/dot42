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

using System;
using Android.App;
using Android.Bluetooth;
using Android.Content;using Android.OS;
using Android.Util;using Android.Views;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace BluetoothChat
{
    /// <summary>
	/// This Activity appears as a dialog. It lists any paired devices and
	/// devices detected in the area after discovery. When a device is chosen
	/// by the user, the MAC address of the device is sent back to the parent
	/// Activity in the result Intent.
	/// </summary>
	[Activity(Label = "@string/select_device", ConfigChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, VisibleInLauncher = false)]
    public class DeviceListActivity : Activity
	{
		// Debugging
		private const string TAG = "DeviceListActivity";
		private const bool D = true;

		// Return Intent extra
		public static string EXTRA_DEVICE_ADDRESS = "device_address";

		// Member fields
		private BluetoothAdapter mBtAdapter;
		private ArrayAdapter<string> mPairedDevicesArrayAdapter;
		private ArrayAdapter<string> mNewDevicesArrayAdapter;
        private readonly MyBroadcastReceiver mReceiver;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DeviceListActivity()
        {
            mReceiver = new MyBroadcastReceiver(this);
        }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Setup the window
			RequestWindowFeature(Window.FEATURE_INDETERMINATE_PROGRESS);
			SetContentView(R.Layout.device_list);

			// Set result CANCELED in case the user backs out
			SetResult(Activity.RESULT_CANCELED);

			// Initialize the button to perform device discovery
			var scanButton = (Button) FindViewById(R.Id.button_scan);
		    scanButton.Click += (s, x) => {
		        doDiscovery();
                ((View)s).SetVisibility(View.GONE);
		    };

			// Initialize array adapters. One for already paired devices and
			// one for newly discovered devices
			mPairedDevicesArrayAdapter = new ArrayAdapter<string>(this, R.Layout.device_name);
			mNewDevicesArrayAdapter = new ArrayAdapter<string>(this, R.Layout.device_name);

			// Find and set up the ListView for paired devices
			var pairedListView = (ListView) FindViewById(R.Id.paired_devices);
			pairedListView.Adapter = mPairedDevicesArrayAdapter;
		    pairedListView.ItemClick += OnDeviceClick;

			// Find and set up the ListView for newly discovered devices
			ListView newDevicesListView = (ListView) FindViewById(R.Id.new_devices);
			newDevicesListView.Adapter = mNewDevicesArrayAdapter;
			newDevicesListView.ItemClick += OnDeviceClick;

			// Register for broadcasts when a device is discovered
			IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
			this.RegisterReceiver(mReceiver, filter);

			// Register for broadcasts when discovery has finished
			filter = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
			this.RegisterReceiver(mReceiver, filter);

			// Get the local Bluetooth adapter
			mBtAdapter = BluetoothAdapter.GetDefaultAdapter();

			// Get a set of currently paired devices
			var pairedDevices = mBtAdapter.BondedDevices;

			// If there are paired devices, add each one to the ArrayAdapter
			if (pairedDevices.Size() > 0)
			{
				FindViewById(R.Id.title_paired_devices).Visibility = View.VISIBLE;
				foreach (BluetoothDevice device in pairedDevices.AsEnumerable())
				{
					mPairedDevicesArrayAdapter.Add(device.Name + "\n" + device.Address);
				}
			}
			else
			{
				string noDevices = Resources.GetText(R.String.none_paired).ToString();
				mPairedDevicesArrayAdapter.Add(noDevices);
			}
		}

		
        protected override void OnDestroy()
		{
			base.OnDestroy();

			// Make sure we're not doing discovery anymore
			if (mBtAdapter != null)
			{
				mBtAdapter.CancelDiscovery();
			}

			// Unregister broadcast listeners
			this.UnregisterReceiver(mReceiver);
		}

		/// <summary>
		/// Start device discover with the BluetoothAdapter
		/// </summary>
		private void doDiscovery()
		{
			if (D)
			{
				Log.D(TAG, "doDiscovery()");
			}

			// Indicate scanning in the title
			SetProgressBarIndeterminateVisibility(true);
			SetTitle(R.String.scanning);

			// Turn on sub-title for new devices
			FindViewById(R.Id.title_new_devices).Visibility = View.VISIBLE;

			// If we're already discovering, stop it
			if (mBtAdapter.IsDiscovering())
			{
				mBtAdapter.CancelDiscovery();
			}

			// Request discover from BluetoothAdapter
			mBtAdapter.StartDiscovery();
		}

        private void OnDeviceClick(object sender, ItemClickEventArgs e)
        {
            // Cancel discovery because it's costly and we're about to connect
            mBtAdapter.CancelDiscovery();

            // Get the device MAC address, which is the last 17 chars in the View
            var info = ((TextView)e.View).GetText().ToString();
            var address = info.Substring(info.Length - 17);

            // Create the result Intent and include the MAC address
            Intent intent = new Intent();
            intent.PutExtra(EXTRA_DEVICE_ADDRESS, address);

            // Set result and finish this Activity
            SetResult(Activity.RESULT_OK, intent);
            Finish();            
        }

		// The BroadcastReceiver that listens for discovered devices and
		// changes the title when discovery is finished
        private class MyBroadcastReceiver : BroadcastReceiver
        {
            private readonly DeviceListActivity activity;

            public MyBroadcastReceiver(DeviceListActivity activity)
            {
                this.activity = activity;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                String action = intent.Action;

                // When discovery finds a device
                if (BluetoothDevice.ACTION_FOUND.Equals(action))
                {
                    // Get the BluetoothDevice object from the Intent
                    BluetoothDevice device = intent.GetParcelableExtra<BluetoothDevice>(BluetoothDevice.EXTRA_DEVICE);
                    // If it's already paired, skip it, because it's been listed already
                    if (device.GetBondState() != BluetoothDevice.BOND_BONDED)
                    {
                        activity.mNewDevicesArrayAdapter.Add(device.GetName() + "\n" + device.GetAddress());
                    }
                    // When discovery is finished, change the Activity title
                }
                else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED.Equals(action))
                {
                    activity.SetProgressBarIndeterminateVisibility(false);
                    activity.SetTitle(R.String.select_device);
                    if (activity.mNewDevicesArrayAdapter.GetCount() == 0)
                    {
                        var noDevices = activity.Resources.GetText(R.String.none_found).ToString();
                        activity.mNewDevicesArrayAdapter.Add(noDevices);
                    }
                }
            }
        }

	}

}