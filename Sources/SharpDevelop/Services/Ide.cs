using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Dot42.AdbLib;
using Dot42.DeviceLib;
using Dot42.FrameworkDefinitions;
using Dot42.Ide;
using Dot42.Ide.Debugger;
using Dot42.Mapping;
using Dot42.SharpDevelop.Debugger;
using ICSharpCode.SharpDevelop.Debugging;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Services
{
	/// <summary>
	/// Description of Ide.
	/// </summary>
	public class Ide : IIde
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
        /// Default ctor
        /// </summary>
		public Ide()
		{
		}
		
		public string LastUsedUniqueId { get;set; }
		
        /// <summary>
        /// Automatically select the last used device when debugging.
        /// </summary>
        public bool? AutoSelectLastUsedDevice { get; set; }

		public void LaunchDebugEngine(string apkPath, Dot42.DebuggerLib.Debugger debugger, Guid debuggerGuid, int launchFlags, Action<LauncherStates, string> stateUpdate)
		{
			var sdDebugger = DebuggerService.CurrentDebugger as Dot42Debugger;            
			if (sdDebugger == null) {
				MessageBox.Show("Dot42 Debugger expected");
				return;
			}
			sdDebugger.Attach(apkPath, debugger, debuggerGuid);
		}
		
		public IIdeOutputPane CreateDebugOutputPane()
		{
			return new IdeOutputPane();
		}

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
