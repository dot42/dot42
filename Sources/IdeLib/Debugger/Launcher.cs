using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dot42.AdbLib;
using Dot42.DebuggerLib;
using Dot42.DeviceLib;
using Dot42.Mapping;
using Dot42.Utility;
using XDebugger = Dot42.DebuggerLib.Debugger;
using Task = System.Threading.Tasks.Task;

namespace Dot42.Ide.Debugger
{
    /// <summary>
    /// Launch VS debug engine
    /// </summary>
    public static class Launcher
    {
        private static readonly Dictionary<Guid, Tuple<DebuggerLib.Debugger, Action<LauncherStates, string>>> debuggers = new Dictionary<Guid, Tuple<XDebugger, Action<LauncherStates, string>>>();
        private static readonly object debuggersLock = new object();
        private static LaunchMonitor monitor;

        /// <summary>
        /// Prepare for receiving a debug launch
        /// </summary>
        internal static void PrepareForLaunch()
        {
            CancelLaunch();
        }

        /// <summary>
        /// Cancel any pending launch.
        /// </summary>
        internal static void CancelLaunch()
        {
            var existing = monitor;
            monitor = null;
            if (existing != null) existing.Dispose();

            lock (debuggersLock)
            {
                debuggers.Clear();
            }
        }

        /// <summary>
        /// Start looking for JDWP processes now.
        /// </summary>
        internal static void StartLaunchMonitor(IIde ide, IDevice device, string apkPath, string packageName, int apiLevel, int launchFlags, Action<LauncherStates, string> stateUpdate, CancellationToken token)
        {
            OutputPaneLog.EnsureLoaded(ide, true);
            OutputPaneLog.EnsureLoaded(ide, false);
            var newMonitor = new LaunchMonitor(ide, device, apkPath, packageName, apiLevel, launchFlags, stateUpdate, token);
            monitor = newMonitor;
            newMonitor.Start();
        }

        /// <summary>
        /// Now launch the actualk VS debugger around the given debugger.
        /// </summary>
        internal static void LaunchVsDebugEngine(IIde ide, string apkPath, DebuggerLib.Debugger debugger, int launchFlags, Action<LauncherStates, string> stateUpdate)
        {
            // Save debugger
            var key = Guid.NewGuid();
            lock (debuggersLock)
            {
                debuggers.Add(key, Tuple.Create(debugger, stateUpdate));
            }

            // Now launch the IDE's debug engine
            ide.LaunchDebugEngine(apkPath, debugger, key, launchFlags, stateUpdate);
        }

        /// <summary>
        /// Gets the debugger by the given guid, while removing it from the list.
        /// </summary>
        public static DebuggerLib.Debugger GetAndRemoveDebugger(Guid guid, out Action<LauncherStates, string> stateUpdate)
        {
            lock (debuggersLock)
            {
                var result = debuggers[guid];
                debuggers.Remove(guid);
                stateUpdate = result.Item2;
                return result.Item1;
            }
        }

        private class LaunchMonitor : IDisposable
        {
            private readonly IIde ide;
            private readonly IDevice device;
            private readonly string apkPath;
            private readonly string packageName;
            private readonly int apiLevel;
            private readonly int launchFlags;
            private readonly Action<LauncherStates, string> stateUpdate;
            private readonly CancellationToken token;

            private JdwpMonitor jdwpMonitor;
            private readonly object processedPidsLock = new object();
            private readonly HashSet<int> processedPids = new HashSet<int>();
            private readonly ConcurrentBag<DebuggerLib.Debugger> debuggers = new ConcurrentBag<DebuggerLib.Debugger>();
            private bool debuggerLaunched;
            private readonly IIdeOutputPane outputPane;

            /// <summary>
            /// Default ctor
            /// </summary>
            public LaunchMonitor(IIde ide, IDevice device, string apkPath, string packageName, int apiLevel, int launchFlags, Action<LauncherStates, string> stateUpdate, CancellationToken token)
            {
                this.ide = ide;
                this.device = device;
                this.apkPath = apkPath;
                this.packageName = packageName;
                this.apiLevel = apiLevel;
                this.launchFlags = launchFlags;
                this.stateUpdate = stateUpdate;
                this.token = token;
                outputPane = ide.CreateDot42OutputPane();
            }

            public void Start()
            {
                // Start JDWP monitor
                jdwpMonitor = new JdwpMonitor(device);
                jdwpMonitor.Start();

                // Wait for initial update
                jdwpMonitor.WaitForInitialUpdate();
                jdwpMonitor.ProcessAdded += (s, x) => { DLog.Debug(DContext.VSDebuggerLauncher, "JDWP Process added {0}", x.Data); Task.Factory.StartNew(() => OnDebuggableProcessAdded(device, x.Data)); };
                jdwpMonitor.ProcessRemoved += (s, x) => DLog.Info(DContext.VSDebuggerLauncher, "JDWP process removed {0}", x.Data);                
            }

            /// <summary>
            /// New JDWP process found, attach debugger to it.
            /// </summary>
            private void OnDebuggableProcessAdded(IDevice device, int pid)
            {
                // Did we see the pid before?
                lock (processedPidsLock)
                {
                    if (!processedPids.Add(pid))
                    {
                        outputPane.LogLine(string.Format("Found debuggable process {0} again, ignoring", pid));
                        return;
                    }
                }

                // Wait a while, it can be a very short lived process
                Thread.Sleep(500);

                // Cancelled?
                if (token.IsCancellationRequested)
                {
                    DLog.Debug(DContext.VSDebuggerLauncher, "Cancel requested");
                    return;                    
                }

                // Check if pid still exists
                var monitor = jdwpMonitor;
                if ((monitor == null) || !monitor.ContainsProcess(pid))
                {
                    DLog.Debug(DContext.VSDebuggerLauncher, "JDWP process gone {0}", pid);
                    return;
                }

                // Create and attach debugger
                outputPane.LogLine(string.Format("Found debuggable process {0}", pid));
                var debugger = new XDebugger(ide.GetFrameworkTypeMap(apiLevel));
                debuggers.Add(debugger);
                debugger.JdwpEvent += OnDebuggerDalvikEvent;
                debugger.Connect(device, pid, new MapFile(), apkPath);
                ide.CurrentDevice = device;
                ide.CurrentProcessId = pid;
            }

            /// <summary>
            /// Debugger has connected.
            /// </summary>
            private void OnDebuggerDalvikEvent(object sender, JdwpEvent e)
            {
                DLog.Debug(DContext.VSDebuggerEvent, "DalvikEvent {0}", e);
                var debugger = (XDebugger)sender;
                if (!debugger.Connected || token.IsCancellationRequested)
                    return;

                if ((debugger.AppName == packageName) && !debuggerLaunched)
                {
                    // Inform status
                    debuggerLaunched = true;
                    outputPane.LogLine("Found process to debug");

                    // We've found the process to debug.
                    var currentMonitor = jdwpMonitor;
                    jdwpMonitor = null;
                    debugger.Attach(currentMonitor);
                    DisposeOtherDebuggers(debugger);

                    // Prepare the debugger
                    stateUpdate(LauncherStates.Attaching, string.Empty);
                    outputPane.LogLine("Preparing debugger launch");
                    try
                    {
                        LaunchVsDebugEngine(ide, apkPath, debugger, launchFlags, stateUpdate);
                    }
                    catch (Exception ex)
                    {
                        stateUpdate(LauncherStates.Error, ex.Message);
                    }
                }
            }

            /// <summary>
            /// Stop and close any open JDWP monitor.
            /// </summary>
            private void StopJdwpMonitor()
            {
                var monitor = jdwpMonitor;
                if (monitor != null)
                {
                    jdwpMonitor = null;
                    monitor.Stop();
                    monitor.Dispose();
                }
            }

            /// <summary>
            /// Disconnect all debuggers except the given one.
            /// </summary>
            private void DisposeOtherDebuggers(XDebugger correctDebugger)
            {
                XDebugger debugger;
                while (debuggers.TryTake(out debugger))
                {
                    if (debugger != correctDebugger)
                    {
                        debugger.Dispose();
                    }
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
                StopJdwpMonitor();
            }
        }
    }
}
