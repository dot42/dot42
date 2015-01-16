using System;
using System.Collections.Generic;
using System.IO;
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
    /// A Device monitor. This connects to the Android Debug Bridge and get device and
    /// debuggable process information from it.
    /// </summary>
    internal sealed class DeviceMonitor
    {
        private readonly byte[] mLengthBuffer = new byte[4];
        private readonly byte[] mLengthBuffer2 = new byte[4];

        private bool mQuit = false;

        private AndroidDebugBridge mServer;

        private SocketChannel mMainAdbConnection = null;
        private bool mMonitoring = false;
        private int mConnectionAttempt = 0;
        private int mRestartAttemptCount = 0;
        private bool mInitialDeviceListDone = false;

        private Selector mSelector;

        private readonly List<Device> mDevices = new List<Device>();

        private readonly List<int> mDebuggerPorts = new List<int>();

        private readonly Dictionary<Client, int> mClientsToReopen = new Dictionary<Client, int>();

        /// <summary>
        /// Creates a new <seealso cref="DeviceMonitor"/> object and links it to the running
        /// <seealso cref="AndroidDebugBridge"/> object. </summary>
        /// <param name="server"> the running <seealso cref="AndroidDebugBridge"/>. </param>
        internal DeviceMonitor(AndroidDebugBridge server)
        {
            mServer = server;

            mDebuggerPorts.Add(DdmPreferences.debugPortBase);
        }

        /// <summary>
        /// Starts the monitoring.
        /// </summary>
        internal void start()
        {
            var thread = new Thread(deviceMonitorLoop);
            thread.Name = "Device List Monitor";
            thread.Start();
        }

        /// <summary>
        /// Stops the monitoring.
        /// </summary>
        internal void stop()
        {
            mQuit = true;

            // wakeup the main loop thread by closing the main connection to adb.
            try
            {
                if (mMainAdbConnection != null)
                {
                    mMainAdbConnection.close();
                }
            }
            catch (IOException)
            {
            }

            // wake up the secondary loop by closing the selector.
            if (mSelector != null)
            {
                mSelector.wakeup();
            }
        }



        /// <summary>
        /// Returns if the monitor is currently connected to the debug bridge server.
        /// @return
        /// </summary>
        internal bool monitoring
        {
            get
            {
                return mMonitoring;
            }
        }

        internal int connectionAttemptCount
        {
            get
            {
                return mConnectionAttempt;
            }
        }

        internal int restartAttemptCount
        {
            get
            {
                return mRestartAttemptCount;
            }
        }

        /// <summary>
        /// Returns the devices.
        /// </summary>
        internal Device[] devices
        {
            get
            {
                lock (mDevices)
                {
                    return mDevices.ToArray();
                }
            }
        }

        internal bool hasInitialDeviceList()
        {
            return mInitialDeviceListDone;
        }

        internal AndroidDebugBridge server
        {
            get
            {
                return mServer;
            }
        }

        internal void addClientToDropAndReopen(Client client, int port)
        {
            lock (mClientsToReopen)
            {
                Log.d("DeviceMonitor", "Adding " + client + " to list of client to reopen (" + port + ").");
                if (!mClientsToReopen.ContainsKey(client))
                {
                    mClientsToReopen.Add(client, port);
                }
            }
            mSelector.wakeup();
        }

        /// <summary>
        /// Monitors the devices. This connects to the Debug Bridge
        /// </summary>
        private void deviceMonitorLoop()
        {
            do
            {
                try
                {
                    if (mMainAdbConnection == null)
                    {
                        Log.d("DeviceMonitor", "Opening adb connection");
                        mMainAdbConnection = openAdbConnection();
                        if (mMainAdbConnection == null)
                        {
                            mConnectionAttempt++;
                            Log.e("DeviceMonitor", "Connection attempts: " + mConnectionAttempt);
                            if (mConnectionAttempt > 10)
                            {
                                if (mServer.startAdb() == false)
                                {
                                    mRestartAttemptCount++;
                                    Log.e("DeviceMonitor", "adb restart attempts: " + mRestartAttemptCount);
                                }
                                else
                                {
                                    mRestartAttemptCount = 0;
                                }
                            }
                            waitABit();
                        }
                        else
                        {
                            Log.d("DeviceMonitor", "Connected to adb for device monitoring");
                            mConnectionAttempt = 0;
                        }
                    }

                    if (mMainAdbConnection != null && mMonitoring == false)
                    {
                        mMonitoring = sendDeviceListMonitoringRequest();
                    }

                    if (mMonitoring)
                    {
                        // read the length of the incoming message
                        int length = readLength(mMainAdbConnection, mLengthBuffer);

                        if (length >= 0)
                        {
                            // read the incoming message
                            processIncomingDeviceData(length);

                            // flag the fact that we have build the list at least once.
                            mInitialDeviceListDone = true;
                        }
                    }
                }
                /*catch (AsynchronousCloseException ace)
                {
                    // this happens because of a call to Quit. We do nothing, and the loop will break.
                }*/
                catch (TimeoutException ioe)
                {
                    handleExpectioninMonitorLoop(ioe);
                }
                catch (IOException ioe)
                {
                    handleExpectioninMonitorLoop(ioe);
                }
            } while (mQuit == false);
        }

        private void handleExpectioninMonitorLoop(Exception e)
        {
            if (mQuit == false)
            {
                if (e is TimeoutException)
                {
                    Log.e("DeviceMonitor", "Adb connection Error: timeout");
                }
                else
                {
                    Log.e("DeviceMonitor", "Adb connection Error:" + e.Message);
                }
                mMonitoring = false;
                if (mMainAdbConnection != null)
                {
                    try
                    {
                        mMainAdbConnection.close();
                    }
                    catch (IOException)
                    {
                        // we can safely ignore that one.
                    }
                    mMainAdbConnection = null;

                    // remove all devices from list
                    // because we are going to call mServer.deviceDisconnected which will acquire this
                    // lock we lock it first, so that the AndroidDebugBridge lock is always locked
                    // first.
                    lock (AndroidDebugBridge.@lock)
                    {
                        lock (mDevices)
                        {
                            for (int n = mDevices.Count - 1; n >= 0; n--)
                            {
                                Device device = mDevices[0];
                                removeDevice(device);
                                mServer.deviceDisconnected(device);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sleeps for a little bit.
        /// </summary>
        private void waitABit()
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        /// <summary>
        /// Attempts to connect to the debug bridge server. </summary>
        /// <returns> a connect socket if success, null otherwise </returns>
        private SocketChannel openAdbConnection()
        {
            Log.d("DeviceMonitor", "Connecting to adb for Device List Monitoring...");

            SocketChannel adbChannel = null;
            try
            {
                adbChannel = SocketChannel.open(AndroidDebugBridge.socketAddress);
                adbChannel.socket().NoDelay = true;
            }
            catch (IOException)
            {
            }

            return adbChannel;
        }

        /// 
        /// <summary>
        /// @return </summary>
        /// <exception cref="IOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private boolean sendDeviceListMonitoringRequest() throws TimeoutException, java.io.IOException
        private bool sendDeviceListMonitoringRequest()
        {
            var request = AdbHelper.formAdbRequest("host:track-devices"); //$NON-NLS-1$

            try
            {
                AdbHelper.write(mMainAdbConnection, request);

                AdbHelper.AdbResponse resp = AdbHelper.readAdbResponse(mMainAdbConnection, false); // readDiagString

                if (resp.okay == false)
                {
                    // request was refused by adb!
                    Log.e("DeviceMonitor", "adb refused request: " + resp.message);
                }

                return resp.okay;
            }
            catch (IOException e)
            {
                Log.e("DeviceMonitor", "Sending Tracking request failed!");
                mMainAdbConnection.close();
                throw e;
            }
        }

        /// <summary>
        /// Processes an incoming device message from the socket </summary>
        /// <param name="socket"> </param>
        /// <param name="length"> </param>
        /// <exception cref="IOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void processIncomingDeviceData(int length) throws java.io.IOException
        private void processIncomingDeviceData(int length)
        {
            List<Device> list = new List<Device>();

            if (length > 0)
            {
                var buffer = new byte[length];
                string result = read(mMainAdbConnection, buffer);

                string[] devices = StringHelperClass.StringSplit(result, "\n", true); //$NON-NLS-1$

                foreach (string d in devices)
                {
                    string[] param = StringHelperClass.StringSplit(d, "\t", true); //$NON-NLS-1$
                    if (param.Length == 2)
                    {
                        // new adb uses only serial numbers to identify devices
                        Device device = new Device(this, param[0], DeviceState.getState(param[1])); //serialnumber

                        //add the device to the list
                        list.Add(device);
                    }
                }
            }

            // now merge the new devices with the old ones.
            updateDevices(list);
        }

        /// <summary>
        ///  Updates the device list with the new items received from the monitoring service.
        /// </summary>
        private void updateDevices(List<Device> newList)
        {
            // because we are going to call mServer.deviceDisconnected which will acquire this lock
            // we lock it first, so that the AndroidDebugBridge lock is always locked first.
            lock (AndroidDebugBridge.@lock)
            {
                // array to store the devices that must be queried for information.
                // it's important to not do it inside the synchronized loop as this could block
                // the whole workspace (this lock is acquired during build too).
                List<Device> devicesToQuery = new List<Device>();
                lock (mDevices)
                {
                    // For each device in the current list, we look for a matching the new list.
                    // * if we find it, we update the current object with whatever new information
                    //   there is
                    //   (mostly state change, if the device becomes ready, we query for build info).
                    //   We also remove the device from the new list to mark it as "processed"
                    // * if we do not find it, we remove it from the current list.
                    // Once this is done, the new list contains device we aren't monitoring yet, so we
                    // add them to the list, and start monitoring them.

                    for (int d = 0; d < mDevices.Count; )
                    {
                        Device device = mDevices[d];

                        // look for a similar device in the new list.
                        int count = newList.Count;
                        bool foundMatch = false;
                        for (int dd = 0; dd < count; dd++)
                        {
                            Device newDevice = newList[dd];
                            // see if it matches in id and serial number.
                            if (newDevice.serialNumber.Equals(device.serialNumber))
                            {
                                foundMatch = true;

                                // update the state if needed.
                                if (device.state != newDevice.state)
                                {
                                    device.state = newDevice.state;
                                    device.update(DeviceConstants.CHANGE_STATE);

                                    // if the device just got ready/online, we need to start
                                    // monitoring it.
                                    if (device.online)
                                    {
                                        if (AndroidDebugBridge.clientSupport == true)
                                        {
                                            if (startMonitoringDevice(device) == false)
                                            {
                                                Log.e("DeviceMonitor", "Failed to start monitoring " + device.serialNumber);
                                            }
                                        }

                                        if (device.propertyCount == 0)
                                        {
                                            devicesToQuery.Add(device);
                                        }
                                    }
                                }

                                // remove the new device from the list since it's been used
                                newList.RemoveAt(dd);
                                break;
                            }
                        }

                        if (foundMatch == false)
                        {
                            // the device is gone, we need to remove it, and keep current index
                            // to process the next one.
                            removeDevice(device);
                            mServer.deviceDisconnected(device);
                        }
                        else
                        {
                            // process the next one
                            d++;
                        }
                    }

                    // at this point we should still have some new devices in newList, so we
                    // process them.
                    foreach (Device newDevice in newList)
                    {
                        // add them to the list
                        mDevices.Add(newDevice);
                        mServer.deviceConnected(newDevice);

                        // start monitoring them.
                        if (AndroidDebugBridge.clientSupport == true)
                        {
                            if (newDevice.online)
                            {
                                startMonitoringDevice(newDevice);
                            }
                        }

                        // look for their build info.
                        if (newDevice.online)
                        {
                            devicesToQuery.Add(newDevice);
                        }
                    }
                }

                // query the new devices for info.
                foreach (Device d in devicesToQuery)
                {
                    queryNewDeviceForInfo(d);
                }
            }
            newList.Clear();
        }

        private void removeDevice(Device device)
        {
            device.clearClientList();
            mDevices.Remove(device);

            SocketChannel channel = device.clientMonitoringSocket;
            if (channel != null)
            {
                try
                {
                    channel.close();
                }
                catch (IOException)
                {
                    // doesn't really matter if the close fails.
                }
            }
        }

        /// <summary>
        /// Queries a device for its build info. </summary>
        /// <param name="device"> the device to query. </param>
        private void queryNewDeviceForInfo(Device device)
        {
            // TODO: do this in a separate thread.
            try
            {
                // first get the list of properties.
                device.executeShellCommand(GetPropReceiver.GETPROP_COMMAND, new GetPropReceiver(device));

                queryNewDeviceForMountingPoint(device, DeviceConstants.MNT_EXTERNAL_STORAGE);
                queryNewDeviceForMountingPoint(device, DeviceConstants.MNT_DATA);
                queryNewDeviceForMountingPoint(device, DeviceConstants.MNT_ROOT);

                // now get the emulator Virtual Device name (if applicable).
                if (device.emulator)
                {
                    EmulatorConsole console = EmulatorConsole.getConsole(device);
                    if (console != null)
                    {
                        device.avdName = console.avdName;
                    }
                }
            }
            catch (TimeoutException)
            {
                Log.w("DeviceMonitor", string.Format("Connection timeout getting info for device {0}", device.serialNumber));

            }
            catch (AdbCommandRejectedException e)
            {
                // This should never happen as we only do this once the device is online.
                Log.w("DeviceMonitor", string.Format("Adb rejected command to get  device {0} info: {1}", device.serialNumber, e.Message));

            }
            catch (ShellCommandUnresponsiveException)
            {
                Log.w("DeviceMonitor", string.Format("Adb shell command took too long returning info for device {0}", device.serialNumber));

            }
            catch (IOException)
            {
                Log.w("DeviceMonitor", string.Format("IO Error getting info for device {0}", device.serialNumber));
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void queryNewDeviceForMountingPoint(final Device device, final String name) throws TimeoutException, AdbCommandRejectedException, ShellCommandUnresponsiveException, java.io.IOException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
        private void queryNewDeviceForMountingPoint(Device device, string name)
        {
            var tempVar = new MultiLineReceiverAnonymousInnerClassHelper();
            /*tempVar.isCancelledDelegateInstance =
                () => {
                    return false;
                };*/

            tempVar.processNewLinesDelegateInstance =
                (string[] lines) => {
                    foreach (string line in lines)
                    {
                        if (line.Length > 0)
                        {
                            // this should be the only one.
                            device.setMountingPoint(name, line);
                        }
                    }
                };
            device.executeShellCommand("echo $" + name, tempVar);
        }

        /// <summary>
        /// Starts a monitoring service for a device. </summary>
        /// <param name="device"> the device to monitor. </param>
        /// <returns> true if success. </returns>
        private bool startMonitoringDevice(Device device)
        {
            SocketChannel socketChannel = openAdbConnection();

            if (socketChannel != null)
            {
                try
                {
                    bool result = sendDeviceMonitoringRequest(socketChannel, device);
                    if (result)
                    {

                        if (mSelector == null)
                        {
                            startDeviceMonitorThread();
                        }

                        device.clientMonitoringSocket = socketChannel;

                        lock (mDevices)
                        {
                            // always wakeup before doing the register. The synchronized block
                            // ensure that the selector won't select() before the end of this block.
                            // @see deviceClientMonitorLoop
                            mSelector.wakeup();

                            socketChannel.configureBlocking(false);
                            socketChannel.register(mSelector, SelectionKey.OP_READ, device);
                        }

                        return true;
                    }
                }
                catch (TimeoutException)
                {
                    try
                    {
                        // attempt to close the socket if needed.
                        socketChannel.close();
                    }
                    catch (IOException)
                    {
                        // we can ignore that one. It may already have been closed.
                    }
                    Log.d("DeviceMonitor", "Connection Failure when starting to monitor device '" + device + "' : timeout");
                }
                catch (AdbCommandRejectedException e)
                {
                    try
                    {
                        // attempt to close the socket if needed.
                        socketChannel.close();
                    }
                    catch (IOException)
                    {
                        // we can ignore that one. It may already have been closed.
                    }
                    Log.d("DeviceMonitor", "Adb refused to start monitoring device '" + device + "' : " + e.Message);
                }
                catch (IOException e)
                {
                    try
                    {
                        // attempt to close the socket if needed.
                        socketChannel.close();
                    }
                    catch (IOException)
                    {
                        // we can ignore that one. It may already have been closed.
                    }
                    Log.d("DeviceMonitor", "Connection Failure when starting to monitor device '" + device + "' : " + e.Message);
                }
            }

            return false;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void startDeviceMonitorThread() throws java.io.IOException
        private void startDeviceMonitorThread()
		{
			mSelector = Selector.open();
            
            var thread = new Thread(deviceClientMonitorLoop);
            thread.Name = "Device Client Monitor";
            thread.Start();
		}

        private void deviceClientMonitorLoop()
        {
            do
            {
                try
                {
                    // This synchronized block stops us from doing the select() if a new
                    // Device is being added.
                    // @see startMonitoringDevice()
                    lock (mDevices)
                    {
                    }

                    int count = mSelector.select();

                    if (mQuit)
                    {
                        return;
                    }

                    lock (mClientsToReopen)
                    {
                        if (mClientsToReopen.Count > 0)
                        {
                            var clients = mClientsToReopen.Keys;
                            MonitorThread monitorThread = MonitorThread.instance;

                            foreach (Client client in clients)
                            {
                                Device device = client.deviceImpl;
                                int pid = client.clientData.pid;

                                monitorThread.dropClient(client, false); // notify

                                // This is kinda bad, but if we don't wait a bit, the client
                                // will never answer the second handshake!
                                waitABit();

                                int port = mClientsToReopen[client];

                                if (port == DebugPortManager.DebugPortProvider.NO_STATIC_PORT)
                                {
                                    port = nextDebuggerPort;
                                }
                                Log.d("DeviceMonitor", "Reopening " + client);
                                openClient(device, pid, port, monitorThread);
                                device.update(DeviceConstants.CHANGE_CLIENT_LIST);
                            }

                            mClientsToReopen.Clear();
                        }
                    }

                    if (count == 0)
                    {
                        continue;
                    }

                    var keys = mSelector.selectedKeys();
                    foreach (var key in keys)
                    {
                        //SelectionKey key = iter.Current;
                        //iter.remove();

                        if (key.valid && key.readable)
                        {
                            object attachment = key.attachment();

                            if (attachment is Device)
                            {
                                Device device = (Device)attachment;

                                SocketChannel socket = device.clientMonitoringSocket;

                                if (socket != null)
                                {
                                    try
                                    {
                                        int length = readLength(socket, mLengthBuffer2);

                                        processIncomingJdwpData(device, socket, length);
                                    }
                                    catch (IOException ioe)
                                    {
                                        Log.d("DeviceMonitor", "Error reading jdwp list: " + ioe.Message);
                                        socket.close();

                                        // restart the monitoring of that device
                                        lock (mDevices)
                                        {
                                            if (mDevices.Contains(device))
                                            {
                                                Log.d("DeviceMonitor", "Restarting monitoring service for " + device);
                                                startMonitoringDevice(device);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    if (mQuit == false)
                    {

                    }
                }

            } while (mQuit == false);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private boolean sendDeviceMonitoringRequest(java.nio.channels.SocketChannel socket, Device device) throws TimeoutException, AdbCommandRejectedException, java.io.IOException
        private bool sendDeviceMonitoringRequest(SocketChannel socket, Device device)
        {

            try
            {
                AdbHelper.setDevice(socket, device);

                var request = AdbHelper.formAdbRequest("track-jdwp"); //$NON-NLS-1$

                AdbHelper.write(socket, request);

                AdbHelper.AdbResponse resp = AdbHelper.readAdbResponse(socket, false); // readDiagString

                if (resp.okay == false)
                {
                    // request was refused by adb!
                    Log.e("DeviceMonitor", "adb refused request: " + resp.message);
                }

                return resp.okay;
            }
            catch (TimeoutException e)
            {
                Log.e("DeviceMonitor", "Sending jdwp tracking request timed out!");
                throw e;
            }
            catch (IOException e)
            {
                Log.e("DeviceMonitor", "Sending jdwp tracking request failed!");
                throw e;
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private void processIncomingJdwpData(Device device, java.nio.channels.SocketChannel monitorSocket, int length) throws java.io.IOException
        private void processIncomingJdwpData(Device device, SocketChannel monitorSocket, int length)
        {
            if (length >= 0)
            {
                // array for the current pids.
                List<int?> pidList = new List<int?>();

                // get the string data if there are any
                if (length > 0)
                {
                    var buffer = new byte[length];
                    string result = read(monitorSocket, buffer);

                    // split each line in its own list and create an array of integer pid
                    string[] pids = StringHelperClass.StringSplit(result, "\n", true); //$NON-NLS-1$

                    foreach (string pid in pids)
                    {
                        try
                        {
                            pidList.Add(Convert.ToInt32(pid));
                        }
                        catch (SystemException)
                        {
                            // looks like this pid is not really a number. Lets ignore it.
                            continue;
                        }
                    }
                }

                MonitorThread monitorThread = MonitorThread.instance;

                // Now we merge the current list with the old one.
                // this is the same mechanism as the merging of the device list.

                // For each client in the current list, we look for a matching the pid in the new list.
                // * if we find it, we do nothing, except removing the pid from its list,
                //   to mark it as "processed"
                // * if we do not find any match, we remove the client from the current list.
                // Once this is done, the new list contains pids for which we don't have clients yet,
                // so we create clients for them, add them to the list, and start monitoring them.

                IList<Client> clients = device.clientList;

                bool changed = false;

                // because MonitorThread#dropClient acquires first the monitorThread lock and then the
                // Device client list lock (when removing the Client from the list), we have to make
                // sure we acquire the locks in the same order, since another thread (MonitorThread),
                // could call dropClient itself.
                lock (monitorThread)
                {
                    lock (clients)
                    {
                        for (int c = 0; c < clients.Count; )
                        {
                            Client client = clients[c];
                            int pid = client.clientData.pid;

                            // look for a matching pid
                            int? match = null;
                            foreach (int? matchingPid in pidList)
                            {
                                if (pid == (int)matchingPid)
                                {
                                    match = matchingPid;
                                    break;
                                }
                            }

                            if (match != null)
                            {
                                pidList.Remove(match);
                                c++; // move on to the next client.
                            }
                            else
                            {
                                // we need to drop the client. the client will remove itself from the
                                // list of its device which is 'clients', so there's no need to
                                // increment c.
                                // We ask the monitor thread to not send notification, as we'll do
                                // it once at the end.
                                monitorThread.dropClient(client, false); // notify
                                changed = true;
                            }
                        }
                    }
                }

                // at this point whatever pid is left in the list needs to be converted into Clients.
                foreach (int newPid in pidList)
                {
                    openClient(device, newPid, nextDebuggerPort, monitorThread);
                    changed = true;
                }

                if (changed)
                {
                    mServer.deviceChanged(device, DeviceConstants.CHANGE_CLIENT_LIST);
                }
            }
        }

        /// <summary>
        /// Opens and creates a new client.
        /// @return
        /// </summary>
        private void openClient(Device device, int pid, int port, MonitorThread monitorThread)
        {

            SocketChannel clientSocket;
            try
            {
                clientSocket = AdbHelper.createPassThroughConnection(AndroidDebugBridge.socketAddress, device, pid);

                // required for Selector
                clientSocket.configureBlocking(false);
            }
            catch (ArgumentException)
            {
                Log.d("DeviceMonitor", "Unknown Jdwp pid: " + pid);
                return;
            }
            catch (TimeoutException)
            {
                Log.w("DeviceMonitor", "Failed to connect to client '" + pid + "': timeout");
                return;
            }
            catch (AdbCommandRejectedException e)
            {
                Log.w("DeviceMonitor", "Adb rejected connection to client '" + pid + "': " + e.Message);
                return;

            }
            catch (Exception ioe)
            {
                Log.w("DeviceMonitor", "Failed to connect to client '" + pid + "': " + ioe.Message);
                return;
            }

            createClient(device, pid, clientSocket, port, monitorThread);
        }

        /// <summary>
        /// Creates a client and register it to the monitor thread </summary>
        /// <param name="device"> </param>
        /// <param name="pid"> </param>
        /// <param name="socket"> </param>
        /// <param name="debuggerPort"> the debugger port. </param>
        /// <param name="monitorThread"> the <seealso cref="MonitorThread"/> object. </param>
        private void createClient(Device device, int pid, SocketChannel socket, int debuggerPort, MonitorThread monitorThread)
        {

            /*
             * Successfully connected to something. Create a Client object, add
             * it to the list, and initiate the JDWP handshake.
             */

            Client client = new Client(device, socket, pid);

            if (client.sendHandshake())
            {
                try
                {
                    if (AndroidDebugBridge.clientSupport)
                    {
                        client.listenForDebugger(debuggerPort);
                    }
                }
                catch (IOException)
                {
                    client.clientData.debuggerConnectionStatus = ClientData.DebuggerStatus.ERROR;
                    Log.e("ddms", "Can't bind to local " + debuggerPort + " for debugger");
                    // oh well
                }

                client.requestAllocationStatus();
            }
            else
            {
                Log.e("ddms", "Handshake with " + client + " failed!");
                /*
                 * The handshake send failed. We could remove it now, but if the
                 * failure is "permanent" we'll just keep banging on it and
                 * getting the same result. Keep it in the list with its "error"
                 * state so we don't try to reopen it.
                 */
            }

            if (client.valid)
            {
                device.addClient(client);
                monitorThread.addClient(client);
            }
            else
            {
                client = null;
            }
        }

        private int nextDebuggerPort
        {
            get
            {
                // get the first port and remove it
                lock (mDebuggerPorts)
                {
                    if (mDebuggerPorts.Count > 0)
                    {
                        int port = mDebuggerPorts[0];

                        // remove it.
                        mDebuggerPorts.Remove(0);

                        // if there's nothing left, add the next port to the list
                        if (mDebuggerPorts.Count == 0)
                        {
                            mDebuggerPorts.Add(port + 1);
                        }

                        return port;
                    }
                }

                return -1;
            }
        }

        internal void addPortToAvailableList(int port)
        {
            if (port > 0)
            {
                lock (mDebuggerPorts)
                {
                    // because there could be case where clients are closed twice, we have to make
                    // sure the port number is not already in the list.
                    if (mDebuggerPorts.IndexOf(port) == -1)
                    {
                        // add the port to the list while keeping it sorted. It's not like there's
                        // going to be tons of objects so we do it linearly.
                        int count = mDebuggerPorts.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (port < mDebuggerPorts[i])
                            {
                                mDebuggerPorts.Insert(i, port);
                                break;
                            }
                        }
                        // TODO: check if we can compact the end of the list.
                    }
                }
            }
        }

        /// <summary>
        /// Reads the length of the next message from a socket. </summary>
        /// <param name="socket"> The <seealso cref="SocketChannel"/> to read from. </param>
        /// <returns> the length, or 0 (zero) if no data is available from the socket. </returns>
        /// <exception cref="IOException"> if the connection failed. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private int readLength(java.nio.channels.SocketChannel socket, byte[] buffer) throws java.io.IOException
        private int readLength(SocketChannel socket, byte[] buffer)
        {
            string msg = read(socket, buffer);

            if (msg != null)
            {
                try
                {
                    return Convert.ToInt32(msg, 16);
                }
                catch (SystemException)
                {
                    // we'll throw an exception below.
                }
            }

            // we receive something we can't read. It's better to reset the connection at this point.
            throw new IOException("Unable to read length");
        }

        /// <summary>
        /// Fills a buffer from a socket. </summary>
        /// <param name="socket"> </param>
        /// <param name="buffer"> </param>
        /// <returns> the content of the buffer as a string, or null if it failed to convert the buffer. </returns>
        /// <exception cref="IOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: private String read(java.nio.channels.SocketChannel socket, byte[] buffer) throws java.io.IOException
        private string read(SocketChannel socket, byte[] buffer)
        {
            ByteBuffer buf = ByteBuffer.wrap(buffer, 0, buffer.Length);

            while (buf.position != buf.limit)
            {
                int count;

                count = socket.read(buf);
                if (count < 0)
                {
                    throw new IOException("EOF");
                }
            }

            try
            {
                return buffer.getString(0, buf.position, AdbHelper.DEFAULT_ENCODING);
            }
            catch (ArgumentException)
            {
                // we'll return null below.
            }

            return null;
        }

    }

}