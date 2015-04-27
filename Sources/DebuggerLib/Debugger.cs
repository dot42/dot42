using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Dot42.AdbLib;
using Dot42.DebuggerLib.Events.Dalvik;
using Dot42.DebuggerLib.Model;
using Dot42.DeviceLib;
using Dot42.Mapping;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// JDWP based debugger.
    /// </summary>
    public partial class Debugger : IDisposable
    {
        private readonly FrameworkTypeMap frameworkTypeMap;
        private JdwpConnection connection;
        private readonly object appNameLock = new object();
        private string appName;
        private const int FirstDebuggerPort = 12600;
        private static int lastDebuggerPort = FirstDebuggerPort;
        private static readonly object lastDebuggerPortLock = new object();
        private bool prepared;
        private DalvikProcess process;
        private MapFile mapFile;
        private JdwpMonitor jdwpMonitor;
        private int pid = -1;
        private string apkFile;

        /// <summary>
        /// Connected state has changed.
        /// </summary>
        public event EventHandler ConnectedChanged;

        /// <summary>
        /// Event source.
        /// </summary>
        public event EventHandler<JdwpEvent> JdwpEvent;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Debugger(FrameworkTypeMap frameworkTypeMap)
        {
            this.frameworkTypeMap = frameworkTypeMap;
            compositeCommandProcessor = new CompositeCommandProcessor(this);
            // Command sets
            VirtualMachine = new VirtualMachineCommandSet(this);
            ReferenceType = new ReferenceTypeCommandSet(this);
            ClassType = new ClassTypeCommandSet(this);
            Method = new MethodCommandSet(this);
            ObjectReference = new ObjectReferenceCommandSet(this);
            StringReference = new StringReferenceCommandSet(this);
            ThreadReference = new ThreadReferenceCommandSet(this);
            StackFrame = new StackFrameCommandSet(this);
            ArrayReference = new ArrayReferenceCommandSet(this);
            EventRequest = new EventRequestCommandSet(this);
            Ddms = new DdmsCommandSet(this);
        }

        /// <summary>
        /// Connect to the VM in the given process id on the given device.
        /// </summary>
        public void Connect(IDevice device, int pid, MapFile mapFile, string apkPath)
        {
            // Disconnect any pending connections
            Disconnect();

            // Cleanup
            process = null;
            this.mapFile = mapFile;
            this.pid = pid;
            this.apkFile = apkPath;

            // Setup forward
            var port = GetFreePort();
            var adb = new Adb();
            adb.ForwardJdwp(device, port, pid);

            // Establish connection
            connection = new JdwpConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port), ChunkHandler, pid, PacketHandler);
            connection.Disconnect += OnConnectionDisconnect;

            // Notify
            ConnectedChanged.Fire(this);
        }

        /// <summary>
        /// Setup the connection in actual debugging state.
        /// </summary>
        public Task PrepareAsync()
        {
            if (prepared)
                return Task.Factory.StartNew(() => { });
            prepared = true;
            return ConnectionOrError.StartDebuggingAsync().ContinueWith(t => Process.PrepareForDebuggingAsync()).Unwrap();
        }

        /// <summary>
        /// Disconnect from any connected VM.
        /// </summary>
        public void Disconnect()
        {
            var conn = connection;
            connection = null;
            prepared = false;
            pid = -1;
            var oldMonitor = jdwpMonitor;
            jdwpMonitor = null;
            if (oldMonitor != null)
            {
                oldMonitor.Stop();
                oldMonitor.Dispose();
            }

            if (conn != null)
            {
                try
                {
                    conn.Close();
                }
                catch
                {
                    // Ignore
                }
            }

            // Notify
            ConnectedChanged.Fire(this);
        }

        /// <summary>
        /// JDWP connection says it is disconnected.
        /// </summary>
        private void OnConnectionDisconnect(object sender, EventArgs e)
        {
            Disconnect();
        }

        /// <summary>
        /// Attach the given JDWP monitor to this debugger.
        /// It will be stopped when this debugger is closed.
        /// </summary>
        public void Attach(JdwpMonitor jdwpMonitor)
        {
            var currentMonitor = this.jdwpMonitor;
            if ((currentMonitor != null) && (currentMonitor != jdwpMonitor))
            {
                currentMonitor.Stop();
                currentMonitor.Dispose();
                this.jdwpMonitor = null;
            }
            this.jdwpMonitor = jdwpMonitor;
            if (jdwpMonitor != null)
            {
                jdwpMonitor.ProcessRemoved += OnJdwpProcessRemoved;
            }
        }

        /// <summary>
        /// Listen to JDWP process changes
        /// </summary>
        private void OnJdwpProcessRemoved(object sender, EventArgs<int> e)
        {
            if (e.Data == pid)
            {
                // Our process has crashed
                DLog.Warning(DContext.DebuggerLibDebugger, "My process (pid:{0}) is gone", e.Data);
                Disconnect();
            }
        }

        /// <summary>
        /// Gets a high level model of the process being debugged inside the virtual machine.
        /// </summary>
        public DalvikProcess Process
        {
            get
            {
                if (process != null) return process;
                if (!Connected) throw new InvalidOperationException("Not connected");
                return process ?? CreateProcess();
            }
            set
            {
                if (process != null)
                    throw new InvalidOperationException("Cannot override process");
                process = value;
            }
        }

        /// <summary>
        /// Create a new instance of the high level model of the process being debugged in the virtual machine.
        /// </summary>
        protected virtual DalvikProcess CreateProcess()
        {
            return new DalvikProcess(this, mapFile, apkFile);
        }

        /// <summary>
        /// Send a HELO DDM chunk.
        /// </summary>
        public void SendHelo()
        {
            ConnectionOrError.SendHelo();
        }

        /// <summary>
        /// Is there a connection?
        /// </summary>
        public bool Connected { get { return connection != null; } }

        /// <summary>
        /// Gets the app name.
        /// Returns null if not available.
        /// </summary>
        public string AppName
        {
            get { return appName; }
        }

        /// <summary>
        /// Sets the appname in a lock.
        /// </summary>
        private void SetAppName(string value)
        {
            lock (appNameLock)
            {
                if (appName == value)
                    return;
                appName = value;
            }
            OnEventAsync(new AppNameChanged(value));
        }

        /// <summary>
        /// Handle non-reply chunks
        /// </summary>
        private void ChunkHandler(Chunk chunk)
        {
            DLog.Debug(DContext.DebuggerLibDebugger, "Handle Chunk {0}", chunk);
            var type = chunk.Type;
            if (type == DdmsCommandSet.HeloType)
            {
                var data = chunk.Data;
                var clientProtocolVersion = data.GetInt();
                var pid = data.GetInt();
                var vmIdentLen = data.GetInt();
                var appNameLen = data.GetInt();
                var vmIdent = data.GetString(vmIdentLen);
                var appName = data.GetString(appNameLen);
                SetAppName(appName);
            }
            else if (type == DdmsCommandSet.ApnmType)
            {
                var data = chunk.Data;
                var appNameLen = data.GetInt();
                var appName = data.GetString(appNameLen);
                SetAppName(appName);
            }
            else if (type == DdmsCommandSet.WaitType)
            {
                var data = chunk.Data;
                var reason = data.GetByte();
                OnEventAsync(new WaitForDebugger(reason));
            }
            else if (type == DdmsCommandSet.ThstType)
            {
                ThreadStatus.HandleTHST(chunk, OnEventAsync);
            }
        }

        /// <summary>
        /// Handle the given non-DDM, non-reply packet.
        /// </summary>
        private void PacketHandler(JdwpPacket packet)
        {
            if ((packet.CommandSet == 64) && (packet.Command == 100))
            {
                // Composite command
                HandleCompositeCommand(packet);
            }            
        }

        /// <summary>
        /// Gets the current connection 
        /// </summary>
        private JdwpConnection ConnectionOrError
        {
            get
            {
                var result = connection;
                if (result == null)
                    throw new NotConnectedException();
                return result;
            }
        }

        /// <summary>
        /// Gets the mapping from java type names to .NET type names for the framework.
        /// </summary>
        public FrameworkTypeMap FrameworkTypeMap
        {
            get { return frameworkTypeMap; }
        }

        /// <summary>
        /// Gets the size info for ID numbers.
        /// </summary>
        internal IdSizeInfo GetIdSizeInfo()
        {
            return ConnectionOrError.GetIdSizeInfo();
        }

        /// <summary>
        /// Gets a free port to use for our debugger connection.
        /// </summary>
        private static int GetFreePort()
        {
            lock (lastDebuggerPortLock)
            {
                lastDebuggerPort++;
                var activeConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
                while (true)
                {
                    if (activeConnections.All(x => x.LocalEndPoint.Port != lastDebuggerPort))
                        return lastDebuggerPort;
                    lastDebuggerPort++;
                }
            }
        }

        /// <summary>
        /// Notify clients of the given event.
        /// </summary>
        private void OnEventAsync(JdwpEvent @event)
        {
            Task.Factory.StartNew(() => JdwpEvent.Fire(this, @event))
                .ContinueWith(task =>
                {
                    DLog.Error(DContext.DebuggerLibJdwpConnection, "OnEventAsync: Internal failure on event processing. IsCancelled={0}. Exception={1}", task.IsCanceled, task.Exception);
                }, 
                TaskContinuationOptions.NotOnRanToCompletion);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            try
            {
                Disconnect();
            }
            finally
            {
                ConnectedChanged = null;
                JdwpEvent = null;
            }
        }
    }
}
