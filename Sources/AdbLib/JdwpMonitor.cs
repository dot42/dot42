using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using Dot42.DeviceLib;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Helper that continues to receive jdwp process id's.
    /// </summary>
    public class JdwpMonitor : Component
    {
        /// <summary>
        /// Fired when a new process is found.
        /// </summary>
        public event EventHandler<EventArgs<int>> ProcessAdded;

        /// <summary>
        /// Fired when a process has been removed.
        /// </summary>
        public event EventHandler<EventArgs<int>> ProcessRemoved;

        private readonly IDevice device;
        private readonly IPEndPoint endPoint;
        private int loopCounter;
        private AdbJdwpRequest request;
        private int errorCount;
        private readonly HashSet<int> pids = new HashSet<int>();
        private readonly object pidsLock = new object();
        private bool enabled;
        private bool receivedInitialUpdate;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpMonitor(IDevice device)
            : this(device, Adb.EndPoint)
        {            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpMonitor(IDevice device, IPEndPoint endPoint)
        {
            this.device = device;
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
        }

        /// <summary>
        /// Wait until the first devices list has been received.
        /// </summary>
        public void WaitForInitialUpdate()
        {
            lock (pidsLock)
            {
                while (!receivedInitialUpdate)
                {
                    Monitor.Wait(pidsLock);
                }
            }
        }

        /// <summary>
        /// Is the given process id still a valid on?
        /// </summary>
        public bool ContainsProcess(int pid)
        {
            lock (pidsLock)
            {
                return pids.Contains(pid);
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
                            using (request = new AdbJdwpRequest(endPoint))
                            {
                                request.TrackJdwpProcessIds(device, CallBack);
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
        /// Process id's found.
        /// </summary>
        private void CallBack(List<int> list)
        {
            List<int> removed;
            var added = new List<int>();

            lock (pidsLock)
            {
                // Look for removed processes
                removed = pids.Where(x => !list.Contains(x)).ToList();
                foreach (var pid in removed)
                {
                    pids.Remove(pid);
                }
                // Look for added processes
                foreach (var pid in list)
                {
                    if (!pids.Contains(pid))
                    {
                        // Process added
                        pids.Add(pid);
                        added.Add(pid);
                    }
                }
            }

            // Notify event listeners
            foreach (var pid in removed)
            {
                ProcessRemoved.Fire(this, new EventArgs<int>(pid));
            }
            foreach (var pid in added)
            {
                ProcessAdded.Fire(this, new EventArgs<int>(pid));
            }

            // Pulse
            lock (pidsLock)
            {
                receivedInitialUpdate = true;
                Monitor.PulseAll(pidsLock);
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
            ProcessAdded = null;
            ProcessRemoved = null;
        }
    }
}
