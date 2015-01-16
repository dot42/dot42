using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Dot42.DdmLib.support;

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

namespace Dot42.DdmLib
{
    /// <summary>
	/// A connection to the host-side android debug bridge (adb)
	/// <p/>This is the central point to communicate with any devices, emulators, or the applications
	/// running on them.
	/// <p/><b><seealso cref="#init(boolean)"/> must be called before anything is done.</b>
	/// </summary>
	public sealed class AndroidDebugBridge
	{

		/*
		 * Minimum and maximum version of adb supported. This correspond to
		 * ADB_SERVER_VERSION found in //device/tools/adb/adb.h
		 */

		private const int ADB_VERSION_MICRO_MIN = 20;
		private const int ADB_VERSION_MICRO_MAX = -1;

		private static readonly Regex sAdbVersion = new Regex("^.*(\\d+)\\.(\\d+)\\.(\\d+)$"); //$NON-NLS-1$

		private const string ADB = "adb"; //$NON-NLS-1$
		private const string DDMS = "ddms"; //$NON-NLS-1$
		private const string SERVER_PORT_ENV_VAR = "ANDROID_ADB_SERVER_PORT"; //$NON-NLS-1$

		// Where to find the ADB bridge.
		internal const string ADB_HOST = "127.0.0.1"; //$NON-NLS-1$
		internal const int ADB_PORT = 5037;

		private static EndPoint sSocketAddr;

		private static AndroidDebugBridge sThis;
		private static bool sInitialized = false;
		private static bool sClientSupport;

	/// <summary>
		/// Full path to adb. </summary>
		private string mAdbOsLocation = null;

		private bool mVersionCheck;

		private bool mStarted = false;

		private DeviceMonitor mDeviceMonitor;

		private static readonly List<IDebugBridgeChangeListener> sBridgeListeners = new List<IDebugBridgeChangeListener>();
		private static readonly List<IDeviceChangeListener> sDeviceListeners = new List<IDeviceChangeListener>();
		private static readonly List<IClientChangeListener> sClientListeners = new List<IClientChangeListener>();

		// lock object for synchronization
		private static readonly object sLock = sBridgeListeners;

		/// <summary>
		/// Classes which implement this interface provide a method that deals
		/// with <seealso cref="AndroidDebugBridge"/> changes.
		/// </summary>
		public interface IDebugBridgeChangeListener
		{
			/// <summary>
			/// Sent when a new <seealso cref="AndroidDebugBridge"/> is connected.
			/// <p/>
			/// This is sent from a non UI thread. </summary>
			/// <param name="bridge"> the new <seealso cref="AndroidDebugBridge"/> object. </param>
			void bridgeChanged(AndroidDebugBridge bridge);
		}

		/// <summary>
		/// Classes which implement this interface provide methods that deal
		/// with <seealso cref="IDevice"/> addition, deletion, and changes.
		/// </summary>
		public interface IDeviceChangeListener
		{
			/// <summary>
			/// Sent when the a device is connected to the <seealso cref="AndroidDebugBridge"/>.
			/// <p/>
			/// This is sent from a non UI thread. </summary>
			/// <param name="device"> the new device. </param>
			void deviceConnected(IDevice device);

			/// <summary>
			/// Sent when the a device is connected to the <seealso cref="AndroidDebugBridge"/>.
			/// <p/>
			/// This is sent from a non UI thread. </summary>
			/// <param name="device"> the new device. </param>
			void deviceDisconnected(IDevice device);

			/// <summary>
			/// Sent when a device data changed, or when clients are started/terminated on the device.
			/// <p/>
			/// This is sent from a non UI thread. </summary>
			/// <param name="device"> the device that was updated. </param>
			/// <param name="changeMask"> the mask describing what changed. It can contain any of the following
			/// values: <seealso cref="IDevice#CHANGE_BUILD_INFO"/>, <seealso cref="IDevice#CHANGE_STATE"/>,
			/// <seealso cref="IDevice#CHANGE_CLIENT_LIST"/> </param>
			void deviceChanged(IDevice device, int changeMask);
		}

		/// <summary>
		/// Classes which implement this interface provide methods that deal
		/// with <seealso cref="Client"/>  changes.
		/// </summary>
		public interface IClientChangeListener
		{
			/// <summary>
			/// Sent when an existing client information changed.
			/// <p/>
			/// This is sent from a non UI thread. </summary>
			/// <param name="client"> the updated client. </param>
			/// <param name="changeMask"> the bit mask describing the changed properties. It can contain
			/// any of the following values: <seealso cref="Client#CHANGE_INFO"/>,
			/// <seealso cref="Client#CHANGE_DEBUGGER_STATUS"/>, <seealso cref="Client#CHANGE_THREAD_MODE"/>,
			/// <seealso cref="Client#CHANGE_THREAD_DATA"/>, <seealso cref="Client#CHANGE_HEAP_MODE"/>,
			/// <seealso cref="Client#CHANGE_HEAP_DATA"/>, <seealso cref="Client#CHANGE_NATIVE_HEAP_DATA"/> </param>
			void clientChanged(Client client, int changeMask);
		}

		/// <summary>
		/// Initializes the <code>ddm</code> library.
		/// <p/>This must be called once <b>before</b> any call to
		/// <seealso cref="#createBridge(String, boolean)"/>.
		/// <p>The library can be initialized in 2 ways:
		/// <ul>
		/// <li>Mode 1: <var>clientSupport</var> == <code>true</code>.<br>The library monitors the
		/// devices and the applications running on them. It will connect to each application, as a
		/// debugger of sort, to be able to interact with them through JDWP packets.</li>
		/// <li>Mode 2: <var>clientSupport</var> == <code>false</code>.<br>The library only monitors
		/// devices. The applications are left untouched, letting other tools built on
		/// <code>ddmlib</code> to connect a debugger to them.</li>
		/// </ul>
		/// <p/><b>Only one tool can run in mode 1 at the same time.</b>
		/// <p/>Note that mode 1 does not prevent debugging of applications running on devices. Mode 1
		/// lets debuggers connect to <code>ddmlib</code> which acts as a proxy between the debuggers and
		/// the applications to debug. See <seealso cref="Client#getDebuggerListenPort()"/>.
		/// <p/>The preferences of <code>ddmlib</code> should also be initialized with whatever default
		/// values were changed from the default values.
		/// <p/>When the application quits, <seealso cref="#terminate()"/> should be called. </summary>
		/// <param name="clientSupport"> Indicates whether the library should enable the monitoring and
		/// interaction with applications running on the devices. </param>
		/// <seealso cref= AndroidDebugBridge#createBridge(String, boolean) </seealso>
		/// <seealso cref= DdmPreferences </seealso>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void init(bool clientSupport)
		{
			if (sInitialized)
			{
				throw new InvalidOperationException("AndroidDebugBridge.init() has already been called.");
			}
			sInitialized = true;
			sClientSupport = clientSupport;

			// Determine port and instantiate socket address.
			initAdbSocketAddr();

			MonitorThread monitorThread = MonitorThread.createInstance();
			monitorThread.start();

			HandleHello.register(monitorThread);
			HandleAppName.register(monitorThread);
			HandleTest.register(monitorThread);
			HandleThread.register(monitorThread);
			HandleHeap.register(monitorThread);
			HandleWait.register(monitorThread);
			HandleProfiling.register(monitorThread);
			HandleNativeHeap.register(monitorThread);
		}

		/// <summary>
		/// Terminates the ddm library. This must be called upon application termination.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void terminate()
		{
			// kill the monitoring services
			if (sThis != null && sThis.mDeviceMonitor != null)
			{
				sThis.mDeviceMonitor.stop();
				sThis.mDeviceMonitor = null;
			}

			MonitorThread monitorThread = MonitorThread.instance;
			if (monitorThread != null)
			{
				monitorThread.quit();
			}

			sInitialized = false;
		}

		/// <summary>
		/// Returns whether the ddmlib is setup to support monitoring and interacting with
		/// <seealso cref="Client"/>s running on the <seealso cref="IDevice"/>s.
		/// </summary>
		internal static bool clientSupport
		{
			get
			{
				return sClientSupport;
			}
		}

		/// <summary>
		/// Returns the socket address of the ADB server on the host.
		/// </summary>
		public static EndPoint socketAddress
		{
			get
			{
				return sSocketAddr;
			}
		}

		/// <summary>
		/// Creates a <seealso cref="AndroidDebugBridge"/> that is not linked to any particular executable.
		/// <p/>This bridge will expect adb to be running. It will not be able to start/stop/restart
		/// adb.
		/// <p/>If a bridge has already been started, it is directly returned with no changes (similar
		/// to calling <seealso cref="#getBridge()"/>). </summary>
		/// <returns> a connected bridge. </returns>
		public static AndroidDebugBridge createBridge()
		{
			lock (sLock)
			{
				if (sThis != null)
				{
					return sThis;
				}

				try
				{
					sThis = new AndroidDebugBridge();
					sThis.start();
				}
				catch (Exception)
				{
					sThis = null;
				}

				// because the listeners could remove themselves from the list while processing
				// their event callback, we make a copy of the list and iterate on it instead of
				// the main list.
				// This mostly happens when the application quits.
				IDebugBridgeChangeListener[] listenersCopy = sBridgeListeners.ToArray();

				// notify the listeners of the change
				foreach (IDebugBridgeChangeListener listener in listenersCopy)
				{
					// we attempt to catch any exception so that a bad listener doesn't kill our
					// thread
					try
					{
						listener.bridgeChanged(sThis);
					}
					catch (Exception e)
					{
						Log.e(DDMS, e);
					}
				}

				return sThis;
			}
		}


		/// <summary>
		/// Creates a new debug bridge from the location of the command line tool.
		/// <p/>
		/// Any existing server will be disconnected, unless the location is the same and
		/// <code>forceNewBridge</code> is set to false. </summary>
		/// <param name="osLocation"> the location of the command line tool 'adb' </param>
		/// <param name="forceNewBridge"> force creation of a new bridge even if one with the same location
		/// already exists. </param>
		/// <returns> a connected bridge. </returns>
		public static AndroidDebugBridge createBridge(string osLocation, bool forceNewBridge)
		{
			lock (sLock)
			{
				if (sThis != null)
				{
					if (sThis.mAdbOsLocation != null && sThis.mAdbOsLocation.Equals(osLocation) && forceNewBridge == false)
					{
						return sThis;
					}
					else
					{
						// stop the current server
						sThis.stop();
					}
				}

				try
				{
					sThis = new AndroidDebugBridge(osLocation);
					sThis.start();
				}
				catch (Exception)
				{
					sThis = null;
				}

				// because the listeners could remove themselves from the list while processing
				// their event callback, we make a copy of the list and iterate on it instead of
				// the main list.
				// This mostly happens when the application quits.
				IDebugBridgeChangeListener[] listenersCopy = sBridgeListeners.ToArray();

				// notify the listeners of the change
				foreach (IDebugBridgeChangeListener listener in listenersCopy)
				{
					// we attempt to catch any exception so that a bad listener doesn't kill our
					// thread
					try
					{
						listener.bridgeChanged(sThis);
					}
					catch (Exception e)
					{
						Log.e(DDMS, e);
					}
				}

				return sThis;
			}
		}

		/// <summary>
		/// Returns the current debug bridge. Can be <code>null</code> if none were created.
		/// </summary>
		public static AndroidDebugBridge bridge
		{
			get
			{
				return sThis;
			}
		}

		/// <summary>
		/// Disconnects the current debug bridge, and destroy the object.
		/// <p/>This also stops the current adb host server.
		/// <p/>
		/// A new object will have to be created with <seealso cref="#createBridge(String, boolean)"/>.
		/// </summary>
		public static void disconnectBridge()
		{
			lock (sLock)
			{
				if (sThis != null)
				{
					sThis.stop();
					sThis = null;

					// because the listeners could remove themselves from the list while processing
					// their event callback, we make a copy of the list and iterate on it instead of
					// the main list.
					// This mostly happens when the application quits.
					IDebugBridgeChangeListener[] listenersCopy = sBridgeListeners.ToArray();

					// notify the listeners.
					foreach (IDebugBridgeChangeListener listener in listenersCopy)
					{
						// we attempt to catch any exception so that a bad listener doesn't kill our
						// thread
						try
						{
							listener.bridgeChanged(sThis);
						}
						catch (Exception e)
						{
							Log.e(DDMS, e);
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds the listener to the collection of listeners who will be notified when a new
		/// <seealso cref="AndroidDebugBridge"/> is connected, by sending it one of the messages defined
		/// in the <seealso cref="IDebugBridgeChangeListener"/> interface. </summary>
		/// <param name="listener"> The listener which should be notified. </param>
		public static void addDebugBridgeChangeListener(IDebugBridgeChangeListener listener)
		{
			lock (sLock)
			{
				if (sBridgeListeners.Contains(listener) == false)
				{
					sBridgeListeners.Add(listener);
					if (sThis != null)
					{
						// we attempt to catch any exception so that a bad listener doesn't kill our
						// thread
						try
						{
							listener.bridgeChanged(sThis);
						}
						catch (Exception e)
						{
							Log.e(DDMS, e);
						}
					}
				}
			}
		}

		/// <summary>
		/// Removes the listener from the collection of listeners who will be notified when a new
		/// <seealso cref="AndroidDebugBridge"/> is started. </summary>
		/// <param name="listener"> The listener which should no longer be notified. </param>
		public static void removeDebugBridgeChangeListener(IDebugBridgeChangeListener listener)
		{
			lock (sLock)
			{
				sBridgeListeners.Remove(listener);
			}
		}

		/// <summary>
		/// Adds the listener to the collection of listeners who will be notified when a <seealso cref="IDevice"/>
		/// is connected, disconnected, or when its properties or its <seealso cref="Client"/> list changed,
		/// by sending it one of the messages defined in the <seealso cref="IDeviceChangeListener"/> interface. </summary>
		/// <param name="listener"> The listener which should be notified. </param>
		public static void addDeviceChangeListener(IDeviceChangeListener listener)
		{
			lock (sLock)
			{
				if (sDeviceListeners.Contains(listener) == false)
				{
					sDeviceListeners.Add(listener);
				}
			}
		}

		/// <summary>
		/// Removes the listener from the collection of listeners who will be notified when a
		/// <seealso cref="IDevice"/> is connected, disconnected, or when its properties or its <seealso cref="Client"/>
		/// list changed. </summary>
		/// <param name="listener"> The listener which should no longer be notified. </param>
		public static void removeDeviceChangeListener(IDeviceChangeListener listener)
		{
			lock (sLock)
			{
				sDeviceListeners.Remove(listener);
			}
		}

		/// <summary>
		/// Adds the listener to the collection of listeners who will be notified when a <seealso cref="Client"/>
		/// property changed, by sending it one of the messages defined in the
		/// <seealso cref="IClientChangeListener"/> interface. </summary>
		/// <param name="listener"> The listener which should be notified. </param>
		public static void addClientChangeListener(IClientChangeListener listener)
		{
			lock (sLock)
			{
				if (sClientListeners.Contains(listener) == false)
				{
					sClientListeners.Add(listener);
				}
			}
		}

		/// <summary>
		/// Removes the listener from the collection of listeners who will be notified when a
		/// <seealso cref="Client"/> property changed. </summary>
		/// <param name="listener"> The listener which should no longer be notified. </param>
		public static void removeClientChangeListener(IClientChangeListener listener)
		{
			lock (sLock)
			{
				sClientListeners.Remove(listener);
			}
		}


		/// <summary>
		/// Returns the devices. </summary>
		/// <seealso cref= #hasInitialDeviceList() </seealso>
		public IDevice[] devices
		{
			get
			{
				lock (sLock)
				{
					if (mDeviceMonitor != null)
					{
						return mDeviceMonitor.devices;
					}
				}
    
				return new IDevice[0];
			}
		}

		/// <summary>
		/// Returns whether the bridge has acquired the initial list from adb after being created.
		/// <p/>Calling <seealso cref="#getDevices()"/> right after <seealso cref="#createBridge(String, boolean)"/> will
		/// generally result in an empty list. This is due to the internal asynchronous communication
		/// mechanism with <code>adb</code> that does not guarantee that the <seealso cref="IDevice"/> list has been
		/// built before the call to <seealso cref="#getDevices()"/>.
		/// <p/>The recommended way to get the list of <seealso cref="IDevice"/> objects is to create a
		/// <seealso cref="IDeviceChangeListener"/> object.
		/// </summary>
		public bool hasInitialDeviceList()
		{
			if (mDeviceMonitor != null)
			{
				return mDeviceMonitor.hasInitialDeviceList();
			}

			return false;
		}

		/// <summary>
		/// Sets the client to accept debugger connection on the custom "Selected debug port". </summary>
		/// <param name="selectedClient"> the client. Can be null. </param>
		public Client selectedClient
		{
			set
			{
				MonitorThread monitorThread = MonitorThread.instance;
				if (monitorThread != null)
				{
					monitorThread.selectedClient = value;
				}
			}
		}

		/// <summary>
		/// Returns whether the <seealso cref="AndroidDebugBridge"/> object is still connected to the adb daemon.
		/// </summary>
		public bool connected
		{
			get
			{
				var monitorThread = MonitorThread.instance;
				if (mDeviceMonitor != null && monitorThread != null)
				{
					return mDeviceMonitor.monitoring && monitorThread.IsAlive;
				}
				return false;
			}
		}

		/// <summary>
		/// Returns the number of times the <seealso cref="AndroidDebugBridge"/> object attempted to connect
		/// to the adb daemon.
		/// </summary>
		public int connectionAttemptCount
		{
			get
			{
				if (mDeviceMonitor != null)
				{
					return mDeviceMonitor.connectionAttemptCount;
				}
				return -1;
			}
		}

		/// <summary>
		/// Returns the number of times the <seealso cref="AndroidDebugBridge"/> object attempted to restart
		/// the adb daemon.
		/// </summary>
		public int restartAttemptCount
		{
			get
			{
				if (mDeviceMonitor != null)
				{
					return mDeviceMonitor.restartAttemptCount;
				}
				return -1;
			}
		}

		/// <summary>
		/// Creates a new bridge. </summary>
		/// <param name="osLocation"> the location of the command line tool </param>
		/// <exception cref="InvalidParameterException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private AndroidDebugBridge(String osLocation) throws java.security.InvalidParameterException
		private AndroidDebugBridge(string osLocation)
		{
			if (string.IsNullOrEmpty(osLocation))
			{
				throw new ArgumentException();
			}
			mAdbOsLocation = osLocation;

			checkAdbVersion();
		}

		/// <summary>
		/// Creates a new bridge not linked to any particular adb executable.
		/// </summary>
		private AndroidDebugBridge()
		{
		}

		/// <summary>
		/// Queries adb for its version number and checks it against <seealso cref="#MIN_VERSION_NUMBER"/> and
		/// <seealso cref="#MAX_VERSION_NUMBER"/>
		/// </summary>
		private void checkAdbVersion()
		{
			// default is bad check
			mVersionCheck = false;

			if (mAdbOsLocation == null)
			{
				return;
			}

			try
			{
				string[] command = new string[2];
				command[0] = mAdbOsLocation;
				command[1] = "version"; //$NON-NLS-1$
				Log.d(DDMS, string.Format("Checking '{0} version'", mAdbOsLocation)); //$NON-NLS-1$
				Process process = Process.Start(command[0], command[1]);

				List<string> errorOutput = new List<string>();
				List<string> stdOutput = new List<string>();
				int status = grabProcessOutput(process, errorOutput, stdOutput, true); // waitForReaders

				if (status != 0)
				{
					StringBuilder builder = new StringBuilder("'adb version' failed!"); //$NON-NLS-1$
					foreach (string error in errorOutput)
					{
						builder.Append('\n');
						builder.Append(error);
					}
					Log.logAndDisplay(Log.LogLevel.ERROR, ADB, builder.ToString());
				}

				// check both stdout and stderr
				bool versionFound = false;
				foreach (string line in stdOutput)
				{
					versionFound = scanVersionLine(line);
					if (versionFound)
					{
						break;
					}
				}
				if (!versionFound)
				{
					foreach (string line in errorOutput)
					{
						versionFound = scanVersionLine(line);
						if (versionFound)
						{
							break;
						}
					}
				}

				if (!versionFound)
				{
					// if we get here, we failed to parse the output.
					StringBuilder builder = new StringBuilder("Failed to parse the output of 'adb version':\n"); //$NON-NLS-1$
					builder.Append("Standard Output was:\n"); //$NON-NLS-1$
					foreach (string line in stdOutput)
					{
						builder.Append(line);
						builder.Append('\n');
					}
					builder.Append("\nError Output was:\n"); //$NON-NLS-1$
					foreach (string line in errorOutput)
					{
						builder.Append(line);
						builder.Append('\n');
					}
					Log.logAndDisplay(Log.LogLevel.ERROR, ADB, builder.ToString());
				}
			}
			catch (IOException e)
			{
				Log.logAndDisplay(Log.LogLevel.ERROR, ADB, "Failed to get the adb version: " + e.Message + " from '" + mAdbOsLocation + "' - exists=" + (System.IO.Directory.Exists(mAdbOsLocation) || System.IO.File.Exists(mAdbOsLocation))); //$NON-NLS-1$ - $NON-NLS-1$
			}
			catch (ThreadInterruptedException)
			{
			}
			finally
			{

			}
		}

		/// <summary>
		/// Scans a line resulting from 'adb version' for a potential version number.
		/// <p/>
		/// If a version number is found, it checks the version number against what is expected
		/// by this version of ddms.
		/// <p/>
		/// Returns true when a version number has been found so that we can stop scanning,
		/// whether the version number is in the acceptable range or not.
		/// </summary>
		/// <param name="line"> The line to scan. </param>
		/// <returns> True if a version number was found (whether it is acceptable or not). </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("all") private boolean scanVersionLine(String line)
		private bool scanVersionLine(string line) // With Eclipse 3.6, replace by @SuppressWarnings("unused")
		{
			if (line != null)
			{
				var matcher = sAdbVersion.Match(line);
				if (matcher.Success)
				{
					int majorVersion = int.Parse(matcher.Groups[0].Value);
					int minorVersion = int.Parse(matcher.Groups[1].Value);
					int microVersion = int.Parse(matcher.Groups[2].Value);

					// check only the micro version for now.
					if (microVersion < ADB_VERSION_MICRO_MIN)
					{
						string message = string.Format("Required minimum version of adb: {0:D}.{1:D}.{2:D}." + "Current version is {0:D}.{1:D}.{3:D}", majorVersion, minorVersion, ADB_VERSION_MICRO_MIN, microVersion); //$NON-NLS-1$ - $NON-NLS-1$
						Log.logAndDisplay(Log.LogLevel.ERROR, ADB, message);
					}
					else if (ADB_VERSION_MICRO_MAX != -1 && microVersion > ADB_VERSION_MICRO_MAX)
					{
						string message = string.Format("Required maximum version of adb: {0:D}.{1:D}.{2:D}." + "Current version is {0:D}.{1:D}.{3:D}", majorVersion, minorVersion, ADB_VERSION_MICRO_MAX, microVersion); //$NON-NLS-1$ - $NON-NLS-1$
						Log.logAndDisplay(Log.LogLevel.ERROR, ADB, message);
					}
					else
					{
						mVersionCheck = true;
					}

					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Starts the debug bridge. </summary>
		/// <returns> true if success. </returns>
		internal bool start()
		{
			if (mAdbOsLocation != null && (mVersionCheck == false || startAdb() == false))
			{
				return false;
			}

			mStarted = true;

			// now that the bridge is connected, we start the underlying services.
			mDeviceMonitor = new DeviceMonitor(this);
			mDeviceMonitor.start();

			return true;
		}

	   /// <summary>
	   /// Kills the debug bridge, and the adb host server. </summary>
	   /// <returns> true if success </returns>
		internal bool stop()
		{
			// if we haven't started we return false;
			if (mStarted == false)
			{
				return false;
			}

			// kill the monitoring services
			mDeviceMonitor.stop();
			mDeviceMonitor = null;

			if (stopAdb() == false)
			{
				return false;
			}

			mStarted = false;
			return true;
		}

		/// <summary>
		/// Restarts adb, but not the services around it. </summary>
		/// <returns> true if success. </returns>
		public bool restart()
		{
			if (mAdbOsLocation == null)
			{
				Log.e(ADB, "Cannot restart adb when AndroidDebugBridge is created without the location of adb."); //$NON-NLS-1$
				return false;
			}

			if (mVersionCheck == false)
			{
				Log.logAndDisplay(Log.LogLevel.ERROR, ADB, "Attempting to restart adb, but version check failed!"); //$NON-NLS-1$
				return false;
			}
			lock (this)
			{
				stopAdb();

				bool restart = startAdb();

				if (restart && mDeviceMonitor == null)
				{
					mDeviceMonitor = new DeviceMonitor(this);
					mDeviceMonitor.start();
				}

				return restart;
			}
		}

		/// <summary>
		/// Notify the listener of a new <seealso cref="IDevice"/>.
		/// <p/>
		/// The notification of the listeners is done in a synchronized block. It is important to
		/// expect the listeners to potentially access various methods of <seealso cref="IDevice"/> as well as
		/// <seealso cref="#getDevices()"/> which use internal locks.
		/// <p/>
		/// For this reason, any call to this method from a method of <seealso cref="DeviceMonitor"/>,
		/// <seealso cref="IDevice"/> which is also inside a synchronized block, should first synchronize on
		/// the <seealso cref="AndroidDebugBridge"/> lock. Access to this lock is done through <seealso cref="#getLock()"/>. </summary>
		/// <param name="device"> the new <code>IDevice</code>. </param>
		/// <seealso cref= #getLock() </seealso>
		internal void deviceConnected(IDevice device)
		{
			// because the listeners could remove themselves from the list while processing
			// their event callback, we make a copy of the list and iterate on it instead of
			// the main list.
			// This mostly happens when the application quits.
			IDeviceChangeListener[] listenersCopy = null;
			lock (sLock)
			{
				listenersCopy = sDeviceListeners.ToArray();
			}

			// Notify the listeners
			foreach (IDeviceChangeListener listener in listenersCopy)
			{
				// we attempt to catch any exception so that a bad listener doesn't kill our
				// thread
				try
				{
					listener.deviceConnected(device);
				}
				catch (Exception e)
				{
					Log.e(DDMS, e);
				}
			}
		}

		/// <summary>
		/// Notify the listener of a disconnected <seealso cref="IDevice"/>.
		/// <p/>
		/// The notification of the listeners is done in a synchronized block. It is important to
		/// expect the listeners to potentially access various methods of <seealso cref="IDevice"/> as well as
		/// <seealso cref="#getDevices()"/> which use internal locks.
		/// <p/>
		/// For this reason, any call to this method from a method of <seealso cref="DeviceMonitor"/>,
		/// <seealso cref="IDevice"/> which is also inside a synchronized block, should first synchronize on
		/// the <seealso cref="AndroidDebugBridge"/> lock. Access to this lock is done through <seealso cref="#getLock()"/>. </summary>
		/// <param name="device"> the disconnected <code>IDevice</code>. </param>
		/// <seealso cref= #getLock() </seealso>
		internal void deviceDisconnected(IDevice device)
		{
			// because the listeners could remove themselves from the list while processing
			// their event callback, we make a copy of the list and iterate on it instead of
			// the main list.
			// This mostly happens when the application quits.
			IDeviceChangeListener[] listenersCopy = null;
			lock (sLock)
			{
				listenersCopy = sDeviceListeners.ToArray();
			}

			// Notify the listeners
			foreach (IDeviceChangeListener listener in listenersCopy)
			{
				// we attempt to catch any exception so that a bad listener doesn't kill our
				// thread
				try
				{
					listener.deviceDisconnected(device);
				}
				catch (Exception e)
				{
					Log.e(DDMS, e);
				}
			}
		}

		/// <summary>
		/// Notify the listener of a modified <seealso cref="IDevice"/>.
		/// <p/>
		/// The notification of the listeners is done in a synchronized block. It is important to
		/// expect the listeners to potentially access various methods of <seealso cref="IDevice"/> as well as
		/// <seealso cref="#getDevices()"/> which use internal locks.
		/// <p/>
		/// For this reason, any call to this method from a method of <seealso cref="DeviceMonitor"/>,
		/// <seealso cref="IDevice"/> which is also inside a synchronized block, should first synchronize on
		/// the <seealso cref="AndroidDebugBridge"/> lock. Access to this lock is done through <seealso cref="#getLock()"/>. </summary>
		/// <param name="device"> the modified <code>IDevice</code>. </param>
		/// <seealso cref= #getLock() </seealso>
		internal void deviceChanged(IDevice device, int changeMask)
		{
			// because the listeners could remove themselves from the list while processing
			// their event callback, we make a copy of the list and iterate on it instead of
			// the main list.
			// This mostly happens when the application quits.
			IDeviceChangeListener[] listenersCopy = null;
			lock (sLock)
			{
				listenersCopy = sDeviceListeners.ToArray();
			}

			// Notify the listeners
			foreach (IDeviceChangeListener listener in listenersCopy)
			{
				// we attempt to catch any exception so that a bad listener doesn't kill our
				// thread
				try
				{
					listener.deviceChanged(device, changeMask);
				}
				catch (Exception e)
				{
					Log.e(DDMS, e);
				}
			}
		}

		/// <summary>
		/// Notify the listener of a modified <seealso cref="Client"/>.
		/// <p/>
		/// The notification of the listeners is done in a synchronized block. It is important to
		/// expect the listeners to potentially access various methods of <seealso cref="IDevice"/> as well as
		/// <seealso cref="#getDevices()"/> which use internal locks.
		/// <p/>
		/// For this reason, any call to this method from a method of <seealso cref="DeviceMonitor"/>,
		/// <seealso cref="IDevice"/> which is also inside a synchronized block, should first synchronize on
		/// the <seealso cref="AndroidDebugBridge"/> lock. Access to this lock is done through <seealso cref="#getLock()"/>. </summary>
		/// <param name="device"> the modified <code>Client</code>. </param>
		/// <param name="changeMask"> the mask indicating what changed in the <code>Client</code> </param>
		/// <seealso cref= #getLock() </seealso>
		internal void clientChanged(Client client, int changeMask)
		{
			// because the listeners could remove themselves from the list while processing
			// their event callback, we make a copy of the list and iterate on it instead of
			// the main list.
			// This mostly happens when the application quits.
			IClientChangeListener[] listenersCopy = null;
			lock (sLock)
			{
				listenersCopy = sClientListeners.ToArray();

			}

			// Notify the listeners
			foreach (IClientChangeListener listener in listenersCopy)
			{
				// we attempt to catch any exception so that a bad listener doesn't kill our
				// thread
				try
				{
					listener.clientChanged(client, changeMask);
				}
				catch (Exception e)
				{
					Log.e(DDMS, e);
				}
			}
		}

		/// <summary>
		/// Returns the <seealso cref="DeviceMonitor"/> object.
		/// </summary>
		internal DeviceMonitor deviceMonitor
		{
			get
			{
				return mDeviceMonitor;
			}
		}

		/// <summary>
		/// Starts the adb host side server. </summary>
		/// <returns> true if success </returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		internal bool startAdb()
		{
			if (mAdbOsLocation == null)
			{
				Log.e(ADB, "Cannot start adb when AndroidDebugBridge is created without the location of adb."); //$NON-NLS-1$
				return false;
			}

			Process proc;
			int status = -1;

			try
			{
				string[] command = new string[2];
				command[0] = mAdbOsLocation;
				command[1] = "start-server"; //$NON-NLS-1$
				Log.d(DDMS, string.Format("Launching '{0} {1}' to ensure ADB is running.", mAdbOsLocation, command[1])); //$NON-NLS-1$
			    var processStartInfo = new ProcessStartInfo(command[0], command[1]);
				if (DdmPreferences.useAdbHost)
				{
					string adbHostValue = DdmPreferences.adbHostValue;
					if (!string.IsNullOrEmpty(adbHostValue))
					{
						//TODO : check that the String is a valid IP address
					    processStartInfo.EnvironmentVariables["ADBHOST"] = adbHostValue;
					}
				}
				proc = Process.Start(processStartInfo);

				List<string> errorOutput = new List<string>();
				List<string> stdOutput = new List<string>();
				status = grabProcessOutput(proc, errorOutput, stdOutput, false); // waitForReaders

			}
			catch (IOException ioe)
			{
				Log.d(DDMS, "Unable to run 'adb': " + ioe.Message); //$NON-NLS-1$
				// we'll return false;
			}
			catch (Exception ie)
			{
				Log.d(DDMS, "Unable to run 'adb': " + ie.Message); //$NON-NLS-1$
				// we'll return false;
			}

			if (status != 0)
			{
				Log.w(DDMS, "'adb start-server' failed -- run manually if necessary"); //$NON-NLS-1$
				return false;
			}

			Log.d(DDMS, "'adb start-server' succeeded"); //$NON-NLS-1$

			return true;
		}

		/// <summary>
		/// Stops the adb host side server. </summary>
		/// <returns> true if success </returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private bool stopAdb()
		{
			if (mAdbOsLocation == null)
			{
				Log.e(ADB, "Cannot stop adb when AndroidDebugBridge is created without the location of adb."); //$NON-NLS-1$
				return false;
			}

			Process proc;
			int status = -1;

			try
			{
				string[] command = new string[2];
				command[0] = mAdbOsLocation;
				command[1] = "kill-server"; //$NON-NLS-1$
				proc = Process.Start(command[0], command[1]);
				proc.WaitForExit();
			    status = proc.ExitCode;
			}
			catch (IOException)
			{
				// we'll return false;
			}
			catch (Exception)
			{
				// we'll return false;
			}

			if (status != 0)
			{
				Log.w(DDMS, "'adb kill-server' failed -- run manually if necessary"); //$NON-NLS-1$
				return false;
			}

			Log.d(DDMS, "'adb kill-server' succeeded"); //$NON-NLS-1$
			return true;
		}

		/// <summary>
		/// Get the stderr/stdout outputs of a process and return when the process is done.
		/// Both <b>must</b> be read or the process will block on windows. </summary>
		/// <param name="process"> The process to get the ouput from </param>
		/// <param name="errorOutput"> The array to store the stderr output. cannot be null. </param>
		/// <param name="stdOutput"> The array to store the stdout output. cannot be null. </param>
		/// <param name="displayStdOut"> If true this will display stdout as well </param>
		/// <param name="waitforReaders"> if true, this will wait for the reader threads. </param>
		/// <returns> the process return code. </returns>
		/// <exception cref="InterruptedException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int grabProcessOutput(final Process process, final java.util.ArrayList<String> errorOutput, final java.util.ArrayList<String> stdOutput, boolean waitforReaders) throws InterruptedException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
		private int grabProcessOutput(Process process, List<string> errorOutput, List<string> stdOutput, bool waitforReaders)
		{
			Debug.Assert(errorOutput != null);
			Debug.Assert(stdOutput != null);
			// read the lines as they come. if null is returned, it's
			// because the process finished

		    ThreadStart runT1 = () => {
		                            // create a buffer to read the stderr output
		                            var reader = process.StandardError;
		                            string line;
		                            while ((line = reader.ReadLine()) != null)
		                            {
		                                Log.e(ADB, line);
		                                errorOutput.Add(line);
		                            }
		                        };
		    var t1 = new Thread(runT1);

		    ThreadStart runT2 = () => {
		                            var reader = process.StandardError;
		                            string line;
		                            while ((line = reader.ReadLine()) != null)
		                            {
		                                Log.d(ADB, line);
		                                stdOutput.Add(line);
		                            }
		                        };
		    var t2 = new Thread(runT2);

			t1.Start();
			t2.Start();

			// it looks like on windows process#waitFor() can return
			// before the thread have filled the arrays, so we wait for both threads and the
			// process itself.
			if (waitforReaders)
			{
				try
				{
					t1.Join();
				}
				catch (ThreadInterruptedException)
				{
				}
				try
				{
					t2.Join();
				}
				catch (ThreadInterruptedException)
				{
				}
			}

			// get the return code from the process
			process.WaitForExit();
		    return process.ExitCode;
		}

		/// <summary>
		/// Returns the singleton lock used by this class to protect any access to the listener.
		/// <p/>
		/// This includes adding/removing listeners, but also notifying listeners of new bridges,
		/// devices, and clients.
		/// </summary>
		internal static object @lock
		{
			get
			{
				return sLock;
			}
		}

		/// <summary>
		/// Instantiates sSocketAddr with the address of the host's adb process.
		/// </summary>
		private static void initAdbSocketAddr()
		{
			try
			{
				int adb_port = determineAndValidateAdbPort();
				sSocketAddr = new DnsEndPoint(ADB_HOST, adb_port);
			}
			catch (ArgumentException)
			{
				// localhost should always be known.
			}
		}

		/// <summary>
		/// Determines port where ADB is expected by looking at an env variable.
		/// <p/>
		/// The value for the environment variable ANDROID_ADB_SERVER_PORT is validated,
		/// IllegalArgumentException is thrown on illegal values.
		/// <p/> </summary>
		/// <returns> The port number where the host's adb should be expected or started. </returns>
		/// <exception cref="IllegalArgumentException"> if ANDROID_ADB_SERVER_PORT has a non-numeric value. </exception>
		private static int determineAndValidateAdbPort()
		{
			string adb_env_var;
			int result = ADB_PORT;
			try
			{
				adb_env_var = Environment.GetEnvironmentVariable(SERVER_PORT_ENV_VAR);

				if (adb_env_var != null)
				{
					adb_env_var = adb_env_var.Trim();
				}

				if (!string.IsNullOrEmpty(adb_env_var))
				{
					// C tools (adb, emulator) accept hex and octal port numbers, so need to accept
					// them too.
					result = adb_env_var.decode();

					if (result <= 0)
					{
						string errMsg = "env var " + SERVER_PORT_ENV_VAR + ": must be >=0, got " + Environment.GetEnvironmentVariable(SERVER_PORT_ENV_VAR); //$NON-NLS-1$ - $NON-NLS-1$
						throw new System.ArgumentException(errMsg);
					}
				}
			}
			catch (SecurityException)
			{
				// A security manager has been installed that doesn't allow access to env vars.
				// So an environment variable might have been set, but we can't tell.
				// Let's log a warning and continue with ADB's default port.
				// The issue is that adb would be started (by the forked process having access
				// to the env vars) on the desired port, but within this process, we can't figure out
				// what that port is. However, a security manager not granting access to env vars
				// but allowing to fork is a rare and interesting configuration, so the right
				// thing seems to be to continue using the default port, as forking is likely to
				// fail later on in the scenario of the security manager.
				Log.w(DDMS, "No access to env variables allowed by current security manager. " + "If you've set ANDROID_ADB_SERVER_PORT: it's being ignored."); //$NON-NLS-1$ - $NON-NLS-1$
			}
            catch (SystemException)
            {
                string errMsg = "env var " + SERVER_PORT_ENV_VAR + ": illegal value '" + Environment.GetEnvironmentVariable(SERVER_PORT_ENV_VAR) + "'"; //$NON-NLS-1$ - $NON-NLS-1$ - $NON-NLS-1$
                throw new System.ArgumentException(errMsg);
            }
            return result;
		}

	}

}