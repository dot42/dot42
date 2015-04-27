using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Dot42.DeviceLib;
using Dot42.FrameworkDefinitions;
using Dot42.Ide;
using Dot42.Ide.Debugger;
using Dot42.Mapping;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Services
{
    internal sealed class Ide : IIde
    {
        /// <summary>
        /// Fired when the current device has changed.
        /// </summary>
        public event EventHandler CurrentDeviceChanged;

        /// <summary>
        /// Fired when the CurrentProcessId has changed.
        /// </summary>
        public event EventHandler CurrentProcessIdChanged;

        private readonly Dictionary<int, FrameworkTypeMap> frameworkTypeMaps = new Dictionary<int, FrameworkTypeMap>();
        private readonly object frameworkTypeMapsLock = new object();
        private int currentProcessId;
        private IDevice currentDevice;

        /// <summary>
        /// Now launch the actual IDE debugger around the given debugger.
        /// </summary>
        public void LaunchDebugEngine(string apkPath, DebuggerLib.Debugger debugger, Guid debuggerGuid, int launchFlags, Action<LauncherStates, string> stateUpdate)
        {
            var info = new VsDebugTargetInfo2[1];
            info[0].cbSize = (uint)Marshal.SizeOf(typeof(VsDebugTargetInfo2));
            info[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            info[0].dwProcessId = 0;
            info[0].dwReserved = 0;
            info[0].bstrOptions = debuggerGuid.ToString("B");
            info[0].bstrExe = Path.GetFullPath(apkPath);
            info[0].bstrEnv = "";
            info[0].bstrArg = null;
            info[0].bstrCurDir = Path.GetDirectoryName(apkPath);
            //info[0].clsidCustom = new Guid(GuidList.items.guidDot42Debugger);
            info[0].LaunchFlags = (uint)launchFlags | (uint)__VSDBGLAUNCHFLAGS2.DBGLAUNCH_StopAtEntryPoint;
            info[0].fSendToOutputWindow = 0;
            info[0].bstrRemoteMachine = null;
            info[0].guidLaunchDebugEngine = new Guid(GuidList.Strings.guidDot42DebuggerId);
            info[0].bstrPortName = "Default Dot42 Port";
            info[0].guidPortSupplier = new Guid(GuidList.Strings.guidDot42PortSupplierId);
            //info[0].guidProcessLanguage = new Guid("{3F5162F8-07C6-11D3-9053-00C04FA302A1}");
            info[0].pDebugEngines = IntPtr.Zero;
            info[0].dwDebugEngineCount = 0;
            info[0].pUnknown = null;
            info[0].guidProcessLanguage = Guid.Empty;
            info[0].hStdError = 0;
            info[0].hStdInput = 0;
            info[0].hStdOutput = 0;

            //var engineGuid = new Guid(GuidList.Strings.guidDot42DebuggerId);
            //var engineArrPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(engineGuid));
            //Marshal.StructureToPtr(engineGuid, engineArrPtr, false);
            //info[0].pDebugEngines = engineArrPtr;
            //info[0].dwDebugEngineCount = 1;

            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VsDebugTargetInfo2)));
            Marshal.StructureToPtr(info[0], ptr, false);

            try
            {
                var d = Package.GetGlobalService(typeof(IVsDebugger)) as IVsDebugger2;
                if (d == null)
                    throw new InvalidOperationException("Cannot find IVsDebugger");
                var rc = d.LaunchDebugTargets2(1, ptr);
                ErrorHandler.ThrowOnFailure(rc);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
                //if (engineArrPtr != IntPtr.Zero)
                //{
                    //Marshal.FreeCoTaskMem(engineArrPtr);
                //}
            }
        }

        /// <summary>
        /// Open or create an output pane for the debug engine.
        /// </summary>
        public IIdeOutputPane CreateDot42OutputPane()
        {
            return new IdeOutputPane();
        }

        /// <summary>
        /// Open or create an output pane for the debug engine.
        /// </summary>
        public IIdeOutputPane CreateDebugOutputPane()
        {
            return new IdeOutputPane(VSConstants.OutputWindowPaneGuid.DebugPane_guid, "Debugging");
        }

        /// <summary>
        /// Gets/sets the unique ID of the last used android device.
        /// </summary>
        public string LastUsedUniqueId
        {
            get; set;
        }

        /// <summary>
        /// Automatically select the last used device when debugging.
        /// </summary>
        public bool? AutoSelectLastUsedDevice { get; set; }

        /// <summary>
        /// The last selected device.
        /// </summary>
        public IDevice CurrentDevice
        {
            get { return currentDevice; }
            set
            {
                var changed = (!ReferenceEquals(currentDevice, value));
                currentDevice = value;
                if (changed)
                {
                    CurrentDeviceChanged.Fire(this);
                }
            }
        }

        /// <summary>
        /// Process ID of the process that is being debugged.
        /// </summary>
        public int CurrentProcessId
        {
            get { return currentProcessId; }
            set
            {
                var changed = (currentProcessId != value);
                currentProcessId = value;
                if (changed)
                {
                    CurrentProcessIdChanged.Fire(this);
                }
            }
        }

        /// <summary>
        /// Create a new selection container
        /// </summary>
        public IIdeSelectionContainer CreateSelectionContainer(IServiceProvider serviceProvider)
        {
            return new IdeSelectionContainer(serviceProvider);
        }

        /// <summary>
        /// Gets a type map for the given android version.
        /// </summary>
        public FrameworkTypeMap GetFrameworkTypeMap(int apiLevel)
        {
            lock (frameworkTypeMapsLock)
            {
                FrameworkTypeMap typeMap;
                if (!frameworkTypeMaps.TryGetValue(apiLevel, out typeMap))
                {
                    typeMap = LoadFrameworkTypeMap(apiLevel);
                    frameworkTypeMaps[apiLevel] = typeMap;
                }
                return typeMap;
            }
        }

        /// <summary>
        /// Load the framework type map for the given level.
        /// </summary>
        private static FrameworkTypeMap LoadFrameworkTypeMap(int apiLevel)
        {
            var framework = Frameworks.Instance.Reverse().FirstOrDefault(x => x.Descriptor.ApiLevel >= apiLevel);
            framework = framework ?? Frameworks.Instance.FirstOrDefault();

            if (framework == null)
                return new FrameworkTypeMap();

            var path = Path.Combine(framework.Folder, "dot42.typemap");
            if (!File.Exists(path))
                return new FrameworkTypeMap();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return new FrameworkTypeMap(stream);
            }
        }
    }
}
