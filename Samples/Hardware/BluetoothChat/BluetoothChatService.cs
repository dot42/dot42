using System.IO;
using Android.Bluetooth;
using Android.Content;
using Android.Os;
using Android.Util;
using Java.Io;
using Java.Util;

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

namespace BluetoothChat
{
	/// <summary>
	/// This class does all the work for setting up and managing Bluetooth
	/// connections with other devices. It has a thread that listens for
	/// incoming connections, a thread for connecting with a device, and a
	/// thread for performing data transmissions when connected.
	/// </summary>
	public sealed class BluetoothChatService
	{
		// Debugging
		private const string TAG = "BluetoothChatService";
		private const bool D = true;

		// Name for the SDP record when creating server socket
		private const string NAME_SECURE = "BluetoothChatSecure";
		private const string NAME_INSECURE = "BluetoothChatInsecure";

		// Unique UUID for this application
		private static readonly UUID MY_UUID_SECURE = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");
		private static readonly UUID MY_UUID_INSECURE = UUID.FromString("8ce255c0-200a-11e0-ac64-0800200c9a66");

		// Member fields
		private readonly BluetoothAdapter mAdapter;
		private readonly Handler mHandler;
		private AcceptThread mSecureAcceptThread;
		private AcceptThread mInsecureAcceptThread;
		private ConnectThread mConnectThread;
		private ConnectedThread mConnectedThread;
		private int mState;

		// Constants that indicate the current connection state
		public const int STATE_NONE = 0; // we're doing nothing
		public const int STATE_LISTEN = 1; // now listening for incoming connections
		public const int STATE_CONNECTING = 2; // now initiating an outgoing connection
		public const int STATE_CONNECTED = 3; // now connected to a remote device

		/// <summary>
		/// Constructor. Prepares a new BluetoothChat session. </summary>
		/// <param name="context">  The UI Activity Context </param>
		/// <param name="handler">  A Handler to send messages back to the UI Activity </param>
		public BluetoothChatService(Context context, Handler handler)
		{
			mAdapter = BluetoothAdapter.GetDefaultAdapter();
			mState = STATE_NONE;
			mHandler = handler;
		}

		/// <summary>
		/// Set the current state of the chat connection </summary>
		/// <param name="state">  An integer defining the current connection state </param>
		//[MethodImpl(MethodImplOptions.Synchronized)]
		internal int State
		{
			set
			{
			    lock (this)
			    {
			        if (D)
			        {
			            Log.D(TAG, "setState() " + mState + " -> " + value);
			        }
			        mState = value;

			        // Give the new value to the Handler so the UI Activity can update
			        mHandler.ObtainMessage(global::BluetoothChat.BluetoothChat.MESSAGE_STATE_CHANGE, value, -1).SendToTarget();
			    }
			}
			get
			{
				return mState;
			}
		}

		/// <summary>
		/// Start the chat service. Specifically start AcceptThread to begin a
		/// </summary>
		public void Start()
		{
		    lock (this)
		    {
		        if (D)
		        {
		            Log.D(TAG, "start");
		        }

		        // Cancel any thread attempting to make a connection
		        if (mConnectThread != null)
		        {
		            mConnectThread.Cancel();
		            mConnectThread = null;
		        }

		        // Cancel any thread currently running a connection
		        if (mConnectedThread != null)
		        {
		            mConnectedThread.Cancel();
		            mConnectedThread = null;
		        }

		        State = STATE_LISTEN;

		        // Start the thread to listen on a BluetoothServerSocket
		        if (mSecureAcceptThread == null)
		        {
		            mSecureAcceptThread = new AcceptThread(this, true);
		            mSecureAcceptThread.Start();
		        }
		        if (mInsecureAcceptThread == null)
		        {
		            mInsecureAcceptThread = new AcceptThread(this, false);
		            mInsecureAcceptThread.Start();
		        }
		    }
		}

		/// <summary>
		/// Start the ConnectThread to initiate a connection to a remote device. </summary>
		/// <param name="device">  The BluetoothDevice to connect </param>
		/// <param name="secure"> Socket Security type - Secure (true) , Insecure (false) </param>
		public void Connect(BluetoothDevice device, bool secure)
		{
		    lock (this)
		    {
		        if (D)
		        {
		            Log.D(TAG, "connect to: " + device);
		        }

		        // Cancel any thread attempting to make a connection
		        if (mState == STATE_CONNECTING)
		        {
		            if (mConnectThread != null)
		            {
		                mConnectThread.Cancel();
		                mConnectThread = null;
		            }
		        }

		        // Cancel any thread currently running a connection
		        if (mConnectedThread != null)
		        {
		            mConnectedThread.Cancel();
		            mConnectedThread = null;
		        }

		        // Start the thread to connect with the given device
		        mConnectThread = new ConnectThread(this, device, secure);
		        mConnectThread.Start();
		        State = STATE_CONNECTING;
		    }
		}

		/// <summary>
		/// Start the ConnectedThread to begin managing a Bluetooth connection </summary>
		/// <param name="socket">  The BluetoothSocket on which the connection was made </param>
		/// <param name="device">  The BluetoothDevice that has been connected </param>
		public void Connected(BluetoothSocket socket, BluetoothDevice device, string socketType)
		{
		    lock (this)
		    {
		        if (D)
		        {
		            Log.D(TAG, "connected, Socket Type:" + socketType);
		        }

		        // Cancel the thread that completed the connection
		        if (mConnectThread != null)
		        {
		            mConnectThread.Cancel();
		            mConnectThread = null;
		        }

		        // Cancel any thread currently running a connection
		        if (mConnectedThread != null)
		        {
		            mConnectedThread.Cancel();
		            mConnectedThread = null;
		        }

		        // Cancel the accept thread because we only want to connect to one device
		        if (mSecureAcceptThread != null)
		        {
		            mSecureAcceptThread.Cancel();
		            mSecureAcceptThread = null;
		        }
		        if (mInsecureAcceptThread != null)
		        {
		            mInsecureAcceptThread.Cancel();
		            mInsecureAcceptThread = null;
		        }

		        // Start the thread to manage the connection and perform transmissions
		        mConnectedThread = new ConnectedThread(this, socket, socketType);
		        mConnectedThread.Start();

		        // Send the name of the connected device back to the UI Activity
		        Message msg = mHandler.ObtainMessage(global::BluetoothChat.BluetoothChat.MESSAGE_DEVICE_NAME);
		        Bundle bundle = new Bundle();
		        bundle.PutString(global::BluetoothChat.BluetoothChat.DEVICE_NAME, device.GetName());
		        msg.SetData(bundle);
		        mHandler.SendMessage(msg);

		        State = STATE_CONNECTED;
		    }
		}

		/// <summary>
		/// Stop all threads
		/// </summary>
		public void Stop()
		{
		    lock (this)
		    {
		        if (D)
		        {
		            Log.D(TAG, "stop");
		        }

		        if (mConnectThread != null)
		        {
		            mConnectThread.Cancel();
		            mConnectThread = null;
		        }

		        if (mConnectedThread != null)
		        {
		            mConnectedThread.Cancel();
		            mConnectedThread = null;
		        }

		        if (mSecureAcceptThread != null)
		        {
		            mSecureAcceptThread.Cancel();
		            mSecureAcceptThread = null;
		        }

		        if (mInsecureAcceptThread != null)
		        {
		            mInsecureAcceptThread.Cancel();
		            mInsecureAcceptThread = null;
		        }
		        State = STATE_NONE;
		    }
		}

		/// <summary>
		/// Write to the ConnectedThread in an unsynchronized manner </summary>
		/// <param name="out"> The bytes to write </param>
		/// <seealso cref= ConnectedThread#write(byte[]) </seealso>
		public void Write(sbyte[] @out)
		{
			// Create temporary object
			ConnectedThread r;
			// Synchronize a copy of the ConnectedThread
			lock (this)
			{
				if (mState != STATE_CONNECTED)
				{
					return;
				}
				r = mConnectedThread;
			}
			// Perform the write unsynchronized
			r.Write(@out);
		}

		/// <summary>
		/// Indicate that the connection attempt failed and notify the UI Activity.
		/// </summary>
		private void ConnectionFailed()
		{
			// Send a failure message back to the Activity
			var msg = mHandler.ObtainMessage(BluetoothChat.MESSAGE_TOAST);
			var bundle = new Bundle();
			bundle.PutString(BluetoothChat.TOAST, "Unable to connect device");
			msg.SetData(bundle);
			mHandler.SendMessage(msg);

			// Start the service over to restart listening mode
			Start();
		}

		/// <summary>
		/// Indicate that the connection was lost and notify the UI Activity.
		/// </summary>
		private void ConnectionLost()
		{
			// Send a failure message back to the Activity
			var msg = mHandler.ObtainMessage(BluetoothChat.MESSAGE_TOAST);
			var bundle = new Bundle();
			bundle.PutString(BluetoothChat.TOAST, "Device connection was lost");
			msg.SetData(bundle);
			mHandler.SendMessage(msg);

			// Start the service over to restart listening mode
			Start();
		}

		/// <summary>
		/// This thread runs while listening for incoming connections. It behaves
		/// like a server-side client. It runs until a connection is accepted
		/// (or until cancelled).
		/// </summary>
		private sealed class AcceptThread : System.Threading.Thread
		{
		    private readonly BluetoothChatService chatService;
			// The local server socket
			private readonly BluetoothServerSocket mmServerSocket;
			private readonly string mSocketType;

			public AcceptThread(BluetoothChatService chatService, bool secure)
			{
			    this.chatService = chatService;
			    BluetoothServerSocket tmp = null;
				mSocketType = secure ? "Secure":"Insecure";

				// Create a new listening server socket
				try
				{
					if (secure)
					{
						tmp = chatService.mAdapter.ListenUsingRfcommWithServiceRecord(NAME_SECURE, MY_UUID_SECURE);
					}
					else
					{
						tmp = chatService.mAdapter.ListenUsingInsecureRfcommWithServiceRecord(NAME_INSECURE, MY_UUID_INSECURE);
					}
				}
				catch (IOException e)
				{
					Log.E(TAG, "Socket Type: " + mSocketType + "listen() failed", e);
				}
				mmServerSocket = tmp;
			}

			public override void Run()
			{
				if (D)
				{
					Log.D(TAG, "Socket Type: " + mSocketType + "BEGIN mAcceptThread" + this);
				}
				Name = "AcceptThread" + mSocketType;

				BluetoothSocket socket = null;

				// Listen to the server socket if we're not connected
                while (chatService.mState != STATE_CONNECTED)
				{
					try
					{
						// This is a blocking call and will only return on a
						// successful connection or an exception
						socket = mmServerSocket.Accept();
					}
					catch (IOException e)
					{
						Log.E(TAG, "Socket Type: " + mSocketType + "accept() failed", e);
						break;
					}

					// If a connection was accepted
					if (socket != null)
					{
						lock (chatService)
						{
							switch (chatService.mState)
							{
							case STATE_LISTEN:
							case STATE_CONNECTING:
								// Situation normal. Start the connected thread.
                                chatService.Connected(socket, socket.RemoteDevice, mSocketType);
								break;
							case STATE_NONE:
							case STATE_CONNECTED:
								// Either not ready or already connected. Terminate new socket.
								try
								{
									socket.Close();
								}
								catch (IOException e)
								{
									Log.E(TAG, "Could not close unwanted socket", e);
								}
								break;
							}
						}
					}
				}
				if (D)
				{
					Log.I(TAG, "END mAcceptThread, socket Type: " + mSocketType);
				}

			}

			public void Cancel()
			{
				if (D)
				{
					Log.D(TAG, "Socket Type" + mSocketType + "cancel " + this);
				}
				try
				{
					mmServerSocket.Close();
				}
				catch (IOException e)
				{
					Log.E(TAG, "Socket Type" + mSocketType + "close() of server failed", e);
				}
			}
		}


		/// <summary>
		/// This thread runs while attempting to make an outgoing connection
		/// with a device. It runs straight through; the connection either
		/// succeeds or fails.
		/// </summary>
		private sealed class ConnectThread : System.Threading.Thread
		{
		    private readonly BluetoothChatService chatService;
			private readonly BluetoothSocket mmSocket;
			private readonly BluetoothDevice mmDevice;
			private string mSocketType;

			public ConnectThread(BluetoothChatService chatService, BluetoothDevice device, bool secure)
			{
			    this.chatService = chatService;
			    mmDevice = device;
				BluetoothSocket tmp = null;
				mSocketType = secure ? "Secure" : "Insecure";

				// Get a BluetoothSocket for a connection with the
				// given BluetoothDevice
				try
				{
					if (secure)
					{
						tmp = device.CreateRfcommSocketToServiceRecord(MY_UUID_SECURE);
					}
					else
					{
						tmp = device.CreateInsecureRfcommSocketToServiceRecord(MY_UUID_INSECURE);
					}
				}
				catch (IOException e)
				{
					Log.E(TAG, "Socket Type: " + mSocketType + "create() failed", e);
				}
				mmSocket = tmp;
			}

			public override void Run()
			{
				Log.I(TAG, "BEGIN mConnectThread SocketType:" + mSocketType);
				Name = "ConnectThread" + mSocketType;

				// Always cancel discovery because it will slow down a connection
				chatService.mAdapter.CancelDiscovery();

				// Make a connection to the BluetoothSocket
				try
				{
					// This is a blocking call and will only return on a
					// successful connection or an exception
					mmSocket.Connect();
				}
				catch (IOException e)
				{
					// Close the socket
					try
					{
						mmSocket.Close();
					}
					catch (IOException e2)
					{
						Log.E(TAG, "unable to close() " + mSocketType + " socket during connection failure", e2);
					}
					chatService.ConnectionFailed();
					return;
				}

				// Reset the ConnectThread because we're done
                lock (chatService)
				{
                    chatService.mConnectThread = null;
				}

				// Start the connected thread
                chatService.Connected(mmSocket, mmDevice, mSocketType);
			}

			public void Cancel()
			{
				try
				{
					mmSocket.Close();
				}
				catch (IOException e)
				{
					Log.E(TAG, "close() of connect " + mSocketType + " socket failed", e);
				}
			}
		}

		/// <summary>
		/// This thread runs during a connection with a remote device.
		/// It handles all incoming and outgoing transmissions.
		/// </summary>
		private sealed class ConnectedThread : System.Threading.Thread
		{
			private readonly BluetoothSocket mmSocket;
			private readonly InputStream mmInStream;
			private readonly OutputStream mmOutStream;
		    private readonly BluetoothChatService chatService;

			public ConnectedThread(BluetoothChatService chatService, BluetoothSocket socket, string socketType)
			{
				Log.D(TAG, "create ConnectedThread: " + socketType);
			    this.chatService = chatService;
			    mmSocket = socket;
				InputStream tmpIn = null;
				OutputStream tmpOut = null;

				// Get the BluetoothSocket input and output streams
				try
				{
					tmpIn = socket.GetInputStream();
					tmpOut = socket.GetOutputStream();
				}
				catch (IOException e)
				{
					Log.E(TAG, "temp sockets not created", e);
				}

				mmInStream = tmpIn;
				mmOutStream = tmpOut;
			}

			public override void Run()
			{
				Log.I(TAG, "BEGIN mConnectedThread");
				var buffer = new sbyte[1024];
				int bytes;

				// Keep listening to the InputStream while connected
				while (true)
				{
					try
					{
						// Read from the InputStream
						bytes = mmInStream.Read(buffer);

						// Send the obtained bytes to the UI Activity
						chatService.mHandler.ObtainMessage(global::BluetoothChat.BluetoothChat.MESSAGE_READ, bytes, -1, buffer).SendToTarget();
					}
					catch (IOException e)
					{
						Log.E(TAG, "disconnected", e);
						chatService.ConnectionLost();
						// Start the service over to restart listening mode
						chatService.Start();
						break;
					}
				}
			}

			/// <summary>
			/// Write to the connected OutStream. </summary>
			/// <param name="buffer">  The bytes to write </param>
			public void Write(sbyte[] buffer)
			{
				try
				{
					mmOutStream.Write(buffer);

					// Share the sent message back to the UI Activity
					chatService.mHandler.ObtainMessage(global::BluetoothChat.BluetoothChat.MESSAGE_WRITE, -1, -1, buffer).SendToTarget();
				}
				catch (IOException e)
				{
					Log.E(TAG, "Exception during write", e);
				}
			}

			public void Cancel()
			{
				try
				{
					mmSocket.Close();
				}
				catch (IOException e)
				{
					Log.E(TAG, "close() of connect socket failed", e);
				}
			}
		}
	}

}