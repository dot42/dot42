using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Helper that continues to receive device connection, disconnection and status updates.
    /// </summary>
    public class DeviceMonitor : Component
    {
        /// <summary>
        /// Fired when a new device is found.
        /// </summary>
        public event EventHandler<EventArgs<AndroidDevice>> DeviceAdded;

        /// <summary>
        /// Fired when a device has been removed.
        /// </summary>
        public event EventHandler<EventArgs<AndroidDevice>> DeviceRemoved;

        /// <summary>
        /// Fired when the status of a device has changed.
        /// </summary>
        public event EventHandler<EventArgs<AndroidDevice>> DeviceStateChanged;

        private readonly IPEndPoint endPoint;
        private int loopCounter;
        private AdbDevicesRequest request;
        private int errorCount;
        private readonly List<AndroidDevice> devices = new List<AndroidDevice>();
        private readonly object devicesLock = new object();
        private bool enabled;
        private bool receivedInitialUpdate;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DeviceMonitor()
            : this(Adb.EndPoint)
        {            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public DeviceMonitor(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        /// <summary>
        /// Is the monitor running?
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled == value)
                    return;
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        /// <summary>
        /// Start a new monitoring thread.
        /// </summary>
        public void Start()
        {
            loopCounter++;
            var thread = new Thread(Run) { IsBackground = true };
            thread.Start();
            enabled = true;
        }

        /// <summary>
        /// Stop any current monitoring loop
        /// </summary>
        public void Stop()
        {
            enabled = false;
            loopCounter++;
            var req = request;
            if (req != null)
            {
                req.Close();
            }
            lock (devicesLock)
            {
                devices.Clear();
            }
        }

        /// <summary>
        /// Wait until the first devices list has been received.
        /// </summary>
        public void WaitForInitialUpdate()
        {
            lock (devicesLock)
            {
                while (!receivedInitialUpdate)
                {
                    Monitor.Wait(devicesLock);
                }
            }
        }

        /// <summary>
        /// Wait until the first devices list has been received.
        /// </summary>
        public bool ReceivedInitialUpdate()
        {
            lock (devicesLock)
            {
                Monitor.Wait(devicesLock, 100);
                return receivedInitialUpdate;
            }
        }

        /// <summary>
        /// Perform the monitor loop
        /// </summary>
        private void Run()
        {
            var initialLoopCounter = loopCounter;
            while (initialLoopCounter == loopCounter)
            {
                try
                {
                    if (errorCount > 0)
                    {
                        // Ensure Adb is started
                        new Adb().StartServer(Adb.Timeout.StartServer);
                        errorCount = 0;
                        Thread.Sleep(500);
                    }
                    else
                    {
                        // Start tracking
                        try
                        {
                            using (request = new AdbDevicesRequest(endPoint))
                            {
                                request.TrackDevices(CallBack);
                            }
                        }
                        finally
                        {
                            request = null;
                        }
                    }
                }
                catch
                {
                    errorCount++;
                }
            }
        }

        /// <summary>
        /// New devices found.
        /// </summary>
        private void CallBack(List<AndroidDevice> list)
        {
            lock (devicesLock)
            {
                // Look for removed devices
                var removed = devices.Where(x => list.All(y => y.Serial != x.Serial)).ToList();
                foreach (var device in removed)
                {
                    devices.Remove(device);
                    DeviceRemoved.Fire(this, new EventArgs<AndroidDevice>(device));
                }
                // Look for added or changed devices
                foreach (var device in list)
                {
                    var serial = device.Serial;
                    var existing = devices.FirstOrDefault(x => x.Serial == serial);
                    if (existing == null)
                    {
                        // Device added
                        devices.Add(device);
                        DeviceAdded.Fire(this, new EventArgs<AndroidDevice>(device));
                    }
                    else if (existing.DeviceState != device.DeviceState)
                    {
                        // State changed
                        devices.Remove(existing);
                        devices.Add(device);
                        DeviceStateChanged.Fire(this, new EventArgs<AndroidDevice>(device));
                    }
                }
                receivedInitialUpdate = true;
                Monitor.PulseAll(devicesLock);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
            base.Dispose(disposing);
        }
    }
}
