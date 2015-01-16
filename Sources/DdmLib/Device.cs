using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dot42.DdmLib.log;
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
	/// A Device. It can be a physical device or an emulator.
	/// </summary>
	internal sealed class Device : IDevice
	{

		private const int INSTALL_TIMEOUT = 2 * 60 * 1000; //2min
		private const int BATTERY_TIMEOUT = 2 * 1000; //2 seconds

	/// <summary>
		/// Emulator Serial Number regexp. </summary>
		internal const string RE_EMULATOR_SN = "emulator-(\\d+)"; //$NON-NLS-1$

	/// <summary>
		/// Serial number of the device </summary>
		private string mSerialNumber = null;

	/// <summary>
		/// Name of the AVD </summary>
		private string mAvdName = null;

	/// <summary>
		/// State of the device. </summary>
		private DeviceState mState = null;

	/// <summary>
		/// Device properties. </summary>
		private readonly IDictionary<string, string> mProperties = new Dictionary<string, string>();
		private readonly IDictionary<string, string> mMountPoints = new Dictionary<string, string>();

		private readonly List<Client> mClients = new List<Client>();
		private DeviceMonitor mMonitor;

		private const string LOG_TAG = "Device";

		/// <summary>
		/// Socket for the connection monitoring client connection/disconnection.
		/// </summary>
		private SocketChannel mSocketChannel;

		private bool mArePropertiesSet = false;

		private int? mLastBatteryLevel = null;
		private long mLastBatteryCheckTime = 0;

		/// <summary>
		/// Output receiver for "pm install package.apk" command line.
		/// </summary>
		private sealed class InstallReceiver : MultiLineReceiver
		{

			private const string SUCCESS_OUTPUT = "Success"; //$NON-NLS-1$
			private static readonly Regex FAILURE_PATTERN = new Regex("Failure\\s+\\[(.*)\\]"); //$NON-NLS-1$

			private string mErrorMessage = null;

			public InstallReceiver()
			{
			}

			public override void processNewLines(string[] lines)
			{
				foreach (string line in lines)
				{
					if (line.Length > 0)
					{
						if (line.StartsWith(SUCCESS_OUTPUT))
						{
							mErrorMessage = null;
						}
						else
						{
							var m = FAILURE_PATTERN.Match(line);
							if (m.Success)
							{
								mErrorMessage = m.Groups[0].Value;
							}
						}
					}
				}
			}

			public override bool cancelled
			{
				get
				{
					return false;
				}
			}

			public string errorMessage
			{
				get
				{
					return mErrorMessage;
				}
			}
		}

		/// <summary>
		/// Output receiver for "dumpsys battery" command line.
		/// </summary>
		private sealed class BatteryReceiver : MultiLineReceiver
		{
            private static readonly Regex BATTERY_LEVEL = new Regex("\\s*level: (\\d+)");
            private static readonly Regex SCALE = new Regex("\\s*scale: (\\d+)");

			private int? mBatteryLevel = null;
			private int? mBatteryScale = null;

			/// <summary>
			/// Get the parsed percent battery level.
			/// @return
			/// </summary>
			public int? batteryLevel
			{
				get
				{
					if (mBatteryLevel != null && mBatteryScale != null)
					{
						return (mBatteryLevel * 100) / mBatteryScale;
					}
					return null;
				}
			}

			public override void processNewLines(string[] lines)
			{
				foreach (string line in lines)
				{
					var batteryMatch = BATTERY_LEVEL.Match(line);
					if (batteryMatch.Success)
					{
						try
						{
							mBatteryLevel = Convert.ToInt32(batteryMatch.group(1));
						}
                        catch (SystemException)
						{
							Log.w(LOG_TAG, string.Format("Failed to parse {0} as an integer", batteryMatch.group(1)));
						}
					}
					var scaleMatch = SCALE.Match(line);
					if (scaleMatch.Success)
					{
						try
						{
							mBatteryScale = Convert.ToInt32(scaleMatch.group(1));
						}
                        catch (SystemException)
						{
							Log.w(LOG_TAG, string.Format("Failed to parse {0} as an integer", batteryMatch.group(1)));
						}
					}
				}
			}

			public override bool cancelled
			{
				get
				{
					return false;
				}
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getSerialNumber()
		 */
		public string serialNumber
		{
			get
			{
				return mSerialNumber;
			}
		}

			/// <summary>
		/// {@inheritDoc} </summary>
		public string avdName
		{
			get
			{
				return mAvdName;
			}
			set
			{
				if (emulator == false)
				{
					throw new System.ArgumentException("Cannot set the AVD name of the device is not an emulator");
				}
    
				mAvdName = value;
			}
		}


		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getState()
		 */
		public DeviceState state
		{
			get
			{
				return mState;
			}
			set
			{
				mState = value;
			}
		}



		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getProperties()
		 */
		public IDictionary<string, string> properties
		{
			get
			{
                // Readonly
				return new Dictionary<string, string>(mProperties);
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getPropertyCount()
		 */
		public int propertyCount
		{
			get
			{
				return mProperties.Count;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getProperty(java.lang.String)
		 */
		public string getProperty(string name)
		{
			return mProperties[name];
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public  bool arePropertiesSet()
		{
			return mArePropertiesSet;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getPropertyCacheOrSync(String name) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
		public  string getPropertyCacheOrSync(string name)
		{
			if (mArePropertiesSet)
			{
				return getProperty(name);
			}
			else
			{
				return getPropertySync(name);
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getPropertySync(String name) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
		public  string getPropertySync(string name)
		{
			CollectingOutputReceiver receiver = new CollectingOutputReceiver();
			executeShellCommand(string.Format("getprop '{0}'", name), receiver);
			string value = receiver.output.Trim();
			if (value.Length == 0)
			{
				return null;
			}
			return value;
		}

		public string getMountPoint(string name)
		{
			return mMountPoints[name];
		}


		public override string ToString()
		{
			return mSerialNumber;
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isOnline()
		 */
		public  bool online
		{
			get
			{
				return mState == DeviceState.ONLINE;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isEmulator()
		 */
		public  bool emulator
		{
			get
			{
				return mSerialNumber.matches(RE_EMULATOR_SN);
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isOffline()
		 */
		public  bool offline
		{
			get
			{
				return mState == DeviceState.OFFLINE;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isBootLoader()
		 */
		public  bool bootLoader
		{
			get
			{
				return mState == DeviceState.BOOTLOADER;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#hasClients()
		 */
		public  bool hasClients()
		{
			return mClients.Count > 0;
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getClients()
		 */
		public  Client[] clients
		{
			get
			{
				lock (mClients)
				{
					return mClients.ToArray();
				}
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getClient(java.lang.String)
		 */
		public  Client getClient(string applicationName)
		{
			lock (mClients)
			{
				foreach (Client c in mClients)
				{
					if (applicationName.Equals(c.clientData.clientDescription))
					{
						return c;
					}
				}

			}

			return null;
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getSyncService()
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SyncService getSyncService() throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public  SyncService syncService
		{
			get
			{
				SyncService syncService = new SyncService(AndroidDebugBridge.socketAddress, this);
				if (syncService.openSync())
				{
					return syncService;
				}
    
				return null;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getFileListingService()
		 */
		public  FileListingService fileListingService
		{
			get
			{
				return new FileListingService(this);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RawImage getScreenshot() throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public  RawImage screenshot
		{
			get
			{
				return AdbHelper.getFrameBuffer(AndroidDebugBridge.socketAddress, this);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void executeShellCommand(String command, IShellOutputReceiver receiver) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
		public  void executeShellCommand(string command, IShellOutputReceiver receiver)
		{
			AdbHelper.executeRemoteCommand(AndroidDebugBridge.socketAddress, command, this, receiver, DdmPreferences.timeOut);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void executeShellCommand(String command, IShellOutputReceiver receiver, int maxTimeToOutputResponse) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
		public  void executeShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse)
		{
			AdbHelper.executeRemoteCommand(AndroidDebugBridge.socketAddress, command, this, receiver, maxTimeToOutputResponse);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void runEventLogService(com.android.ddmlib.log.LogReceiver receiver) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public  void runEventLogService(LogReceiver receiver)
		{
			AdbHelper.runEventLogService(AndroidDebugBridge.socketAddress, this, receiver);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void runLogService(String logname, com.android.ddmlib.log.LogReceiver receiver) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public  void runLogService(string logname, LogReceiver receiver)
		{
			AdbHelper.runLogService(AndroidDebugBridge.socketAddress, this, logname, receiver);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createForward(int localPort, int remotePort) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public  void createForward(int localPort, int remotePort)
		{
			AdbHelper.createForward(AndroidDebugBridge.socketAddress, this, localPort, remotePort);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void removeForward(int localPort, int remotePort) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public  void removeForward(int localPort, int remotePort)
		{
			AdbHelper.removeForward(AndroidDebugBridge.socketAddress, this, localPort, remotePort);
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getClientName(int)
		 */
		public  string getClientName(int pid)
		{
			lock (mClients)
			{
				foreach (Client c in mClients)
				{
					if (c.clientData.pid == pid)
					{
						return c.clientData.clientDescription;
					}
				}
			}

			return null;
		}


		internal Device(DeviceMonitor monitor, string serialNumber, DeviceState deviceState)
		{
			mMonitor = monitor;
			mSerialNumber = serialNumber;
			mState = deviceState;
		}

		internal DeviceMonitor monitor
		{
			get
			{
				return mMonitor;
			}
		}

		internal void addClient(Client client)
		{
			lock (mClients)
			{
				mClients.Add(client);
			}
		}

		internal IList<Client> clientList
		{
			get
			{
				return mClients;
			}
		}

		internal bool hasClient(int pid)
		{
			lock (mClients)
			{
				foreach (Client client in mClients)
				{
					if (client.clientData.pid == pid)
					{
						return true;
					}
				}
			}

			return false;
		}

		internal void clearClientList()
		{
			lock (mClients)
			{
				mClients.Clear();
			}
		}

		/// <summary>
		/// Sets the client monitoring socket. </summary>
		/// <param name="socketChannel"> the sockets </param>
		internal SocketChannel clientMonitoringSocket
		{
			set
			{
				mSocketChannel = value;
			}
			get
			{
				return mSocketChannel;
			}
		}


		/// <summary>
		/// Removes a <seealso cref="Client"/> from the list. </summary>
		/// <param name="client"> the client to remove. </param>
		/// <param name="notify"> Whether or not to notify the listeners of a change. </param>
		internal void removeClient(Client client, bool notify)
		{
			mMonitor.addPortToAvailableList(client.debuggerListenPort);
			lock (mClients)
			{
				mClients.Remove(client);
			}
			if (notify)
			{
                mMonitor.server.deviceChanged(this, DeviceConstants.CHANGE_CLIENT_LIST);
			}
		}

		internal void update(int changeMask)
		{
			if ((changeMask & DeviceConstants.CHANGE_BUILD_INFO) != 0)
			{
				mArePropertiesSet = true;
			}
			mMonitor.server.deviceChanged(this, changeMask);
		}

		internal void update(Client client, int changeMask)
		{
			mMonitor.server.clientChanged(client, changeMask);
		}

		internal void addProperty(string label, string value)
		{
			mProperties.Add(label, value);
		}

		internal void setMountingPoint(string name, string value)
		{
			mMountPoints.Add(name, value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pushFile(String local, String remote) throws java.io.IOException, AdbCommandRejectedException, TimeoutException, SyncException
		public void pushFile(string local, string remote)
		{
			SyncService sync = null;
			try
			{
				string targetFileName = getFileName(local);

				Log.d(targetFileName, string.Format("Uploading {0} onto device '{1}'", targetFileName, serialNumber));

				sync = syncService;
				if (sync != null)
				{
					string message = string.Format("Uploading file onto device '{0}'", serialNumber);
					Log.d(LOG_TAG, message);
					sync.pushFile(local, remote, SyncService.nullProgressMonitor);
				}
				else
				{
					throw new IOException("Unable to open sync connection!");
				}
			}
			catch (TimeoutException e)
			{
				Log.e(LOG_TAG, "Error during Sync: timeout.");
				throw e;

			}
			catch (SyncException e)
			{
				Log.e(LOG_TAG, string.Format("Error during Sync: {0}", e.Message));
				throw e;

			}
			catch (IOException e)
			{
				Log.e(LOG_TAG, string.Format("Error during Sync: {0}", e.Message));
				throw e;

			}
			finally
			{
				if (sync != null)
				{
					sync.close();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pullFile(String remote, String local) throws java.io.IOException, AdbCommandRejectedException, TimeoutException, SyncException
		public void pullFile(string remote, string local)
		{
			SyncService sync = null;
			try
			{
				string targetFileName = getFileName(remote);

				Log.d(targetFileName, string.Format("Downloading {0} from device '{1}'", targetFileName, serialNumber));

				sync = syncService;
				if (sync != null)
				{
					string message = string.Format("Downloding file from device '{0}'", serialNumber);
					Log.d(LOG_TAG, message);
					sync.pullFile(remote, local, SyncService.nullProgressMonitor);
				}
				else
				{
					throw new IOException("Unable to open sync connection!");
				}
			}
			catch (TimeoutException e)
			{
				Log.e(LOG_TAG, "Error during Sync: timeout.");
				throw e;

			}
			catch (SyncException e)
			{
				Log.e(LOG_TAG, string.Format("Error during Sync: {0}", e.Message));
				throw e;

			}
			catch (IOException e)
			{
				Log.e(LOG_TAG, string.Format("Error during Sync: {0}", e.Message));
				throw e;

			}
			finally
			{
				if (sync != null)
				{
					sync.close();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String installPackage(String packageFilePath, boolean reinstall, String... extraArgs) throws InstallException
		public string installPackage(string packageFilePath, bool reinstall, params string[] extraArgs)
		{
			try
			{
				string remoteFilePath = syncPackageToDevice(packageFilePath);
				string result = installRemotePackage(remoteFilePath, reinstall, extraArgs);
				removeRemotePackage(remoteFilePath);
				return result;
			}
			catch (IOException e)
			{
				throw new InstallException(e);
			}
			catch (AdbCommandRejectedException e)
			{
				throw new InstallException(e);
			}
			catch (TimeoutException e)
			{
				throw new InstallException(e);
			}
			catch (SyncException e)
			{
				throw new InstallException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String syncPackageToDevice(String localFilePath) throws java.io.IOException, AdbCommandRejectedException, TimeoutException, SyncException
		public string syncPackageToDevice(string localFilePath)
		{
			SyncService sync = null;
			try
			{
				string packageFileName = getFileName(localFilePath);
				string remoteFilePath = string.Format("/data/local/tmp/{0}", packageFileName); //$NON-NLS-1$

				Log.d(packageFileName, string.Format("Uploading {0} onto device '{1}'", packageFileName, serialNumber));

				sync = syncService;
				if (sync != null)
				{
					string message = string.Format("Uploading file onto device '{0}'", serialNumber);
					Log.d(LOG_TAG, message);
					sync.pushFile(localFilePath, remoteFilePath, SyncService.nullProgressMonitor);
				}
				else
				{
					throw new IOException("Unable to open sync connection!");
				}
				return remoteFilePath;
			}
			catch (TimeoutException e)
			{
				Log.e(LOG_TAG, "Error during Sync: timeout.");
				throw e;

			}
			catch (SyncException e)
			{
				Log.e(LOG_TAG, string.Format("Error during Sync: {0}", e.Message));
				throw e;

			}
			catch (IOException e)
			{
				Log.e(LOG_TAG, string.Format("Error during Sync: {0}", e.Message));
				throw e;

			}
			finally
			{
				if (sync != null)
				{
					sync.close();
				}
			}
		}

		/// <summary>
		/// Helper method to retrieve the file name given a local file path </summary>
		/// <param name="filePath"> full directory path to file </param>
		/// <returns> <seealso cref="String"/> file name </returns>
		private string getFileName(string filePath)
		{
			return (new System.IO.FileInfo(filePath)).Name;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String installRemotePackage(String remoteFilePath, boolean reinstall, String... extraArgs) throws InstallException
		public string installRemotePackage(string remoteFilePath, bool reinstall, params string[] extraArgs)
		{
			try
			{
				InstallReceiver receiver = new InstallReceiver();
				StringBuilder optionString = new StringBuilder();
				if (reinstall)
				{
					optionString.Append("-r ");
				}
				foreach (string arg in extraArgs)
				{
					optionString.Append(arg);
					optionString.Append(' ');
				}
				string cmd = string.Format("pm install {0} \"{1}\"", optionString.ToString(), remoteFilePath);
				executeShellCommand(cmd, receiver, INSTALL_TIMEOUT);
				return receiver.errorMessage;
			}
			catch (TimeoutException e)
			{
				throw new InstallException(e);
			}
			catch (AdbCommandRejectedException e)
			{
				throw new InstallException(e);
			}
			catch (ShellCommandUnresponsiveException e)
			{
				throw new InstallException(e);
			}
			catch (IOException e)
			{
				throw new InstallException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void removeRemotePackage(String remoteFilePath) throws InstallException
		public void removeRemotePackage(string remoteFilePath)
		{
			try
			{
				executeShellCommand("rm " + remoteFilePath, new NullOutputReceiver(), INSTALL_TIMEOUT);
			}
			catch (IOException e)
			{
				throw new InstallException(e);
			}
			catch (TimeoutException e)
			{
				throw new InstallException(e);
			}
			catch (AdbCommandRejectedException e)
			{
				throw new InstallException(e);
			}
			catch (ShellCommandUnresponsiveException e)
			{
				throw new InstallException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String uninstallPackage(String packageName) throws InstallException
		public string uninstallPackage(string packageName)
		{
			try
			{
				InstallReceiver receiver = new InstallReceiver();
				executeShellCommand("pm uninstall " + packageName, receiver, INSTALL_TIMEOUT);
				return receiver.errorMessage;
			}
			catch (TimeoutException e)
			{
				throw new InstallException(e);
			}
			catch (AdbCommandRejectedException e)
			{
				throw new InstallException(e);
			}
			catch (ShellCommandUnresponsiveException e)
			{
				throw new InstallException(e);
			}
			catch (IOException e)
			{
				throw new InstallException(e);
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#reboot()
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reboot(String into) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
		public void reboot(string into)
		{
			AdbHelper.reboot(into, AndroidDebugBridge.socketAddress, this);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Integer getBatteryLevel() throws TimeoutException, AdbCommandRejectedException, java.io.IOException, ShellCommandUnresponsiveException
		public int? batteryLevel
		{
			get
			{
				// use default of 5 minutes
				return getBatteryLevel(5 * 60 * 1000);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Integer getBatteryLevel(long freshnessMs) throws TimeoutException, AdbCommandRejectedException, java.io.IOException, ShellCommandUnresponsiveException
		public int? getBatteryLevel(long freshnessMs)
		{
			if (mLastBatteryLevel != null && mLastBatteryCheckTime > (Environment.TickCount - freshnessMs))
			{
				return mLastBatteryLevel;
			}
			BatteryReceiver receiver = new BatteryReceiver();
			executeShellCommand("dumpsys battery", receiver, BATTERY_TIMEOUT);
			mLastBatteryLevel = receiver.batteryLevel;
			mLastBatteryCheckTime = Environment.TickCount;
			return mLastBatteryLevel;
		}
	}

}