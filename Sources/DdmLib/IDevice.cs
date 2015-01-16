using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.DdmLib.log;

/*
 * Copyright (C) 2008 The Android Open Source Project
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
    /// The state of a device.
    /// </summary>
    public sealed class DeviceState
    {
        public static readonly DeviceState BOOTLOADER = new DeviceState("bootloader"); //$NON-NLS-1$
        public static readonly DeviceState OFFLINE = new DeviceState("offline"); //$NON-NLS-1$
        public static readonly DeviceState ONLINE = new DeviceState("device"); //$NON-NLS-1$
        public static readonly DeviceState RECOVERY = new DeviceState("recovery"); //$NON-NLS-1$

        private static readonly DeviceState[] values = new[] { BOOTLOADER, OFFLINE, ONLINE, RECOVERY };

        private String mState;

        DeviceState(String state)
        {
            mState = state;
        }

        /// <summary>
        /// Returns a <seealso cref="DeviceState"/> from the string returned by <code>adb devices</code>.
        /// </summary>
        /// <param name="state"> the device state. </param>
        /// <returns> a <seealso cref="DeviceState"/> object or <code>null</code> if the state is unknown. </returns>
        public static DeviceState getState(String state)
        {
            return values.FirstOrDefault(x => x.mState == state);
        }
    }

    public class DeviceConstants
    {
        		public const String PROP_BUILD_VERSION = "ro.build.version.release";
        		public const String PROP_BUILD_API_LEVEL = "ro.build.version.sdk";
        		public const String PROP_BUILD_CODENAME = "ro.build.version.codename";

        		public const String PROP_DEBUGGABLE = "ro.debuggable";

         ///<summary>Serial number of the first connected emulator. </summary>
        		public const String FIRST_EMULATOR_SN = "emulator-5554"; //$NON-NLS-1$
         ///<summary>Device change bit mask: <seealso cref="DeviceState"/> change. </summary>
        		public const int CHANGE_STATE = 0x0001;
         ///<summary>Device change bit mask: <seealso cref="Client"/> list change. </summary>
        		public const int CHANGE_CLIENT_LIST = 0x0002;
         ///<summary>Device change bit mask: build info change. </summary>
        		public const int CHANGE_BUILD_INFO = 0x0004;

         ///@deprecated Use <seealso cref="#PROP_BUILD_API_LEVEL"/>. 
        		public const String PROP_BUILD_VERSION_NUMBER = PROP_BUILD_API_LEVEL;

        		public const String MNT_EXTERNAL_STORAGE = "EXTERNAL_STORAGE"; //$NON-NLS-1$
        		public const String MNT_ROOT = "ANDROID_ROOT"; //$NON-NLS-1$
        		public const String MNT_DATA = "ANDROID_DATA"; //$NON-NLS-1$
        
    }

	/// <summary>
	///  A Device. It can be a physical device or an emulator.
	/// </summary>
	public interface IDevice
	{

		/// <summary>
		/// Returns the serial number of the device.
		/// </summary>
		string serialNumber {get;}

		/// <summary>
		/// Returns the name of the AVD the emulator is running.
		/// <p/>This is only valid if <seealso cref="#isEmulator()"/> returns true.
		/// <p/>If the emulator is not running any AVD (for instance it's running from an Android source
		/// tree build), this method will return "<code>&lt;build&gt;</code>".
		/// </summary>
		/// <returns> the name of the AVD or <code>null</code> if there isn't any. </returns>
		string avdName {get;}

		/// <summary>
		/// Returns the state of the device.
		/// </summary>
		DeviceState state {get;}

		/// <summary>
		/// Returns the device properties. It contains the whole output of 'getprop'
		/// </summary>
		IDictionary<string, string> properties {get;}

		/// <summary>
		/// Returns the number of property for this device.
		/// </summary>
		int propertyCount {get;}

		/// <summary>
		/// Returns the cached property value.
		/// </summary>
		/// <param name="name"> the name of the value to return. </param>
		/// <returns> the value or <code>null</code> if the property does not exist or has not yet been
		/// cached. </returns>
		string getProperty(string name);

		/// <summary>
		/// Returns <code>true></code> if properties have been cached
		/// </summary>
		bool arePropertiesSet();

		/// <summary>
		/// A variant of <seealso cref="#getProperty(String)"/> that will attempt to retrieve the given
		/// property from device directly, without using cache.
		/// </summary>
		/// <param name="name"> the name of the value to return. </param>
		/// <returns> the value or <code>null</code> if the property does not exist </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> in case the shell command doesn't send output for a
		///             given time. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getPropertySync(String name) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException;
		string getPropertySync(string name);

		/// <summary>
		/// A combination of <seealso cref="#getProperty(String)"/> and <seealso cref="#getPropertySync(String)"/> that
		/// will attempt to retrieve the property from cache if available, and if not, will query the
		/// device directly.
		/// </summary>
		/// <param name="name"> the name of the value to return. </param>
		/// <returns> the value or <code>null</code> if the property does not exist </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> in case the shell command doesn't send output for a
		///             given time. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getPropertyCacheOrSync(String name) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException;
		string getPropertyCacheOrSync(string name);

		/// <summary>
		/// Returns a mount point.
		/// </summary>
		/// <param name="name"> the name of the mount point to return
		/// </param>
		/// <seealso cref= #MNT_EXTERNAL_STORAGE </seealso>
		/// <seealso cref= #MNT_ROOT </seealso>
		/// <seealso cref= #MNT_DATA </seealso>
		string getMountPoint(string name);

		/// <summary>
		/// Returns if the device is ready.
		/// </summary>
		/// <returns> <code>true</code> if <seealso cref="#getState()"/> returns <seealso cref="DeviceState#ONLINE"/>. </returns>
		bool online {get;}

		/// <summary>
		/// Returns <code>true</code> if the device is an emulator.
		/// </summary>
		bool emulator {get;}

		/// <summary>
		/// Returns if the device is offline.
		/// </summary>
		/// <returns> <code>true</code> if <seealso cref="#getState()"/> returns <seealso cref="DeviceState#OFFLINE"/>. </returns>
		bool offline {get;}

		/// <summary>
		/// Returns if the device is in bootloader mode.
		/// </summary>
		/// <returns> <code>true</code> if <seealso cref="#getState()"/> returns <seealso cref="DeviceState#BOOTLOADER"/>. </returns>
		bool bootLoader {get;}

		/// <summary>
		/// Returns whether the <seealso cref="Device"/> has <seealso cref="Client"/>s.
		/// </summary>
		bool hasClients();

		/// <summary>
		/// Returns the array of clients.
		/// </summary>
		Client[] clients {get;}

		/// <summary>
		/// Returns a <seealso cref="Client"/> by its application name.
		/// </summary>
		/// <param name="applicationName"> the name of the application </param>
		/// <returns> the <code>Client</code> object or <code>null</code> if no match was found. </returns>
		Client getClient(string applicationName);

		/// <summary>
		/// Returns a <seealso cref="SyncService"/> object to push / pull files to and from the device.
		/// </summary>
		/// <returns> <code>null</code> if the SyncService couldn't be created. This can happen if adb
		///            refuse to open the connection because the <seealso cref="IDevice"/> is invalid
		///            (or got disconnected). </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> if the connection with adb failed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SyncService getSyncService() throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		SyncService syncService {get;}

		/// <summary>
		/// Returns a <seealso cref="FileListingService"/> for this device.
		/// </summary>
		FileListingService fileListingService {get;}

		/// <summary>
		/// Takes a screen shot of the device and returns it as a <seealso cref="RawImage"/>.
		/// </summary>
		/// <returns> the screenshot as a <code>RawImage</code> or <code>null</code> if something
		///            went wrong. </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RawImage getScreenshot() throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		RawImage screenshot {get;}

		/// <summary>
		/// Executes a shell command on the device, and sends the result to a <var>receiver</var>
		/// <p/>This is similar to calling
		/// <code>executeShellCommand(command, receiver, DdmPreferences.getTimeOut())</code>.
		/// </summary>
		/// <param name="command"> the shell command to execute </param>
		/// <param name="receiver"> the <seealso cref="IShellOutputReceiver"/> that will receives the output of the shell
		///            command </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> in case the shell command doesn't send output
		///            for a given time. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection.
		/// </exception>
		/// <seealso cref= #executeShellCommand(String, IShellOutputReceiver, int) </seealso>
		/// <seealso cref= DdmPreferences#getTimeOut() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void executeShellCommand(String command, IShellOutputReceiver receiver) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException;
		void executeShellCommand(string command, IShellOutputReceiver receiver);

		/// <summary>
		/// Executes a shell command on the device, and sends the result to a <var>receiver</var>.
		/// <p/><var>maxTimeToOutputResponse</var> is used as a maximum waiting time when expecting the
		/// command output from the device.<br>
		/// At any time, if the shell command does not output anything for a period longer than
		/// <var>maxTimeToOutputResponse</var>, then the method will throw
		/// <seealso cref="ShellCommandUnresponsiveException"/>.
		/// <p/>For commands like log output, a <var>maxTimeToOutputResponse</var> value of 0, meaning
		/// that the method will never throw and will block until the receiver's
		/// <seealso cref="IShellOutputReceiver#isCancelled()"/> returns <code>true</code>, should be
		/// used.
		/// </summary>
		/// <param name="command"> the shell command to execute </param>
		/// <param name="receiver"> the <seealso cref="IShellOutputReceiver"/> that will receives the output of the shell
		///            command </param>
		/// <param name="maxTimeToOutputResponse"> the maximum amount of time during which the command is allowed
		///            to not output any response. A value of 0 means the method will wait forever
		///            (until the <var>receiver</var> cancels the execution) for command output and
		///            never throw. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection when sending the command. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command. </exception>
		/// <exception cref="ShellCommandUnresponsiveException"> in case the shell command doesn't send any output
		///            for a period longer than <var>maxTimeToOutputResponse</var>. </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection.
		/// </exception>
		/// <seealso cref= DdmPreferences#getTimeOut() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void executeShellCommand(String command, IShellOutputReceiver receiver, int maxTimeToOutputResponse) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException;
		void executeShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse);

		/// <summary>
		/// Runs the event log service and outputs the event log to the <seealso cref="LogReceiver"/>.
		/// <p/>This call is blocking until <seealso cref="LogReceiver#isCancelled()"/> returns true. </summary>
		/// <param name="receiver"> the receiver to receive the event log entries. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. This can only be thrown if the
		/// timeout happens during setup. Once logs start being received, no timeout will occur as it's
		/// not possible to detect a difference between no log and timeout. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void runEventLogService(com.android.ddmlib.log.LogReceiver receiver) throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		void runEventLogService(LogReceiver receiver);

		/// <summary>
		/// Runs the log service for the given log and outputs the log to the <seealso cref="LogReceiver"/>.
		/// <p/>This call is blocking until <seealso cref="LogReceiver#isCancelled()"/> returns true.
		/// </summary>
		/// <param name="logname"> the logname of the log to read from. </param>
		/// <param name="receiver"> the receiver to receive the event log entries. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. This can only be thrown if the
		///            timeout happens during setup. Once logs start being received, no timeout will
		///            occur as it's not possible to detect a difference between no log and timeout. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void runLogService(String logname, com.android.ddmlib.log.LogReceiver receiver) throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		void runLogService(string logname, LogReceiver receiver);

		/// <summary>
		/// Creates a port forwarding between a local and a remote port.
		/// </summary>
		/// <param name="localPort"> the local port to forward </param>
		/// <param name="remotePort"> the remote port. </param>
		/// <returns> <code>true</code> if success. </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createForward(int localPort, int remotePort) throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		void createForward(int localPort, int remotePort);

		/// <summary>
		/// Removes a port forwarding between a local and a remote port.
		/// </summary>
		/// <param name="localPort"> the local port to forward </param>
		/// <param name="remotePort"> the remote port. </param>
		/// <returns> <code>true</code> if success. </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void removeForward(int localPort, int remotePort) throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		void removeForward(int localPort, int remotePort);

		/// <summary>
		/// Returns the name of the client by pid or <code>null</code> if pid is unknown </summary>
		/// <param name="pid"> the pid of the client. </param>
		string getClientName(int pid);

		/// <summary>
		/// Push a single file. </summary>
		/// <param name="local"> the local filepath. </param>
		/// <param name="remote"> The remote filepath.
		/// </param>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
		/// <exception cref="SyncException"> if file could not be pushed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pushFile(String local, String remote) throws java.io.IOException, AdbCommandRejectedException, TimeoutException, SyncException;
		void pushFile(string local, string remote);

		/// <summary>
		/// Pulls a single file.
		/// </summary>
		/// <param name="remote"> the full path to the remote file </param>
		/// <param name="local"> The local destination.
		/// </param>
		/// <exception cref="IOException"> in case of an IO exception. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="TimeoutException"> in case of a timeout reading responses from the device. </exception>
		/// <exception cref="SyncException"> in case of a sync exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pullFile(String remote, String local) throws java.io.IOException, AdbCommandRejectedException, TimeoutException, SyncException;
		void pullFile(string remote, string local);

		/// <summary>
		/// Installs an Android application on device. This is a helper method that combines the
		/// syncPackageToDevice, installRemotePackage, and removePackage steps
		/// </summary>
		/// <param name="packageFilePath"> the absolute file system path to file on local host to install </param>
		/// <param name="reinstall"> set to <code>true</code> if re-install of app should be performed </param>
		/// <param name="extraArgs"> optional extra arguments to pass. See 'adb shell pm install --help' for
		///            available options. </param>
		/// <returns> a <seealso cref="String"/> with an error code, or <code>null</code> if success. </returns>
		/// <exception cref="InstallException"> if the installation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String installPackage(String packageFilePath, boolean reinstall, String... extraArgs) throws InstallException;
		string installPackage(string packageFilePath, bool reinstall, params string[] extraArgs);

		/// <summary>
		/// Pushes a file to device
		/// </summary>
		/// <param name="localFilePath"> the absolute path to file on local host </param>
		/// <returns> <seealso cref="String"/> destination path on device for file </returns>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> in case of I/O error on the connection. </exception>
		/// <exception cref="SyncException"> if an error happens during the push of the package on the device. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String syncPackageToDevice(String localFilePath) throws TimeoutException, AdbCommandRejectedException, java.io.IOException, SyncException;
		string syncPackageToDevice(string localFilePath);

		/// <summary>
		/// Installs the application package that was pushed to a temporary location on the device.
		/// </summary>
		/// <param name="remoteFilePath"> absolute file path to package file on device </param>
		/// <param name="reinstall"> set to <code>true</code> if re-install of app should be performed </param>
		/// <param name="extraArgs"> optional extra arguments to pass. See 'adb shell pm install --help' for
		///            available options. </param>
		/// <exception cref="InstallException"> if the installation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String installRemotePackage(String remoteFilePath, boolean reinstall, String... extraArgs) throws InstallException;
		string installRemotePackage(string remoteFilePath, bool reinstall, params string[] extraArgs);

		/// <summary>
		/// Removes a file from device.
		/// </summary>
		/// <param name="remoteFilePath"> path on device of file to remove </param>
		/// <exception cref="InstallException"> if the installation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void removeRemotePackage(String remoteFilePath) throws InstallException;
		void removeRemotePackage(string remoteFilePath);

		/// <summary>
		/// Uninstalls an package from the device.
		/// </summary>
		/// <param name="packageName"> the Android application package name to uninstall </param>
		/// <returns> a <seealso cref="String"/> with an error code, or <code>null</code> if success. </returns>
		/// <exception cref="InstallException"> if the uninstallation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String uninstallPackage(String packageName) throws InstallException;
		string uninstallPackage(string packageName);

		/// <summary>
		/// Reboot the device.
		/// </summary>
		/// <param name="into"> the bootloader name to reboot into, or null to just reboot the device. </param>
		/// <exception cref="TimeoutException"> in case of timeout on the connection. </exception>
		/// <exception cref="AdbCommandRejectedException"> if adb rejects the command </exception>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void reboot(String into) throws TimeoutException, AdbCommandRejectedException, java.io.IOException;
		void reboot(string into);

		/// <summary>
		/// Return the device's battery level, from 0 to 100 percent.
		/// <p/>
		/// The battery level may be cached. Only queries the device for its
		/// battery level if 5 minutes have expired since the last successful query.
		/// </summary>
		/// <returns> the battery level or <code>null</code> if it could not be retrieved </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Integer getBatteryLevel() throws TimeoutException, AdbCommandRejectedException, java.io.IOException, ShellCommandUnresponsiveException;
		int? batteryLevel {get;}

		/// <summary>
		/// Return the device's battery level, from 0 to 100 percent.
		/// <p/>
		/// The battery level may be cached. Only queries the device for its
		/// battery level if <code>freshnessMs</code> ms have expired since the last successful query.
		/// </summary>
		/// <param name="freshnessMs"> </param>
		/// <returns> the battery level or <code>null</code> if it could not be retrieved </returns>
		/// <exception cref="ShellCommandUnresponsiveException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Integer getBatteryLevel(long freshnessMs) throws TimeoutException, AdbCommandRejectedException, java.io.IOException, ShellCommandUnresponsiveException;
		int? getBatteryLevel(long freshnessMs);

	}

}