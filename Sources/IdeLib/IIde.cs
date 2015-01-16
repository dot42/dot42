using System;
using Dot42.AdbLib;
using Dot42.DeviceLib;
using Dot42.Ide.Debugger;
using Dot42.Mapping;

namespace Dot42.Ide
{
    public interface IIde
    {
        /// <summary>
        /// Fired when the current device has changed.
        /// </summary>
        event EventHandler CurrentDeviceChanged;

        /// <summary>
        /// Fired when the CurrentProcessId has changed.
        /// </summary>
        event EventHandler CurrentProcessIdChanged;

        /// <summary>
        /// Now launch the actual IDE debugger around the given debugger.
        /// </summary>
        void LaunchDebugEngine(string apkPath, DebuggerLib.Debugger debugger, Guid debuggerGuid, int launchFlags, Action<LauncherStates, string> stateUpdate);

        /// <summary>
        /// Open or create an output pane for the debug engine.
        /// </summary>
        IIdeOutputPane CreateDebugOutputPane();

        /// <summary>
        /// Gets/sets the unique ID of the last used android device.
        /// </summary>
        string LastUsedUniqueId { get; set; }

        /// <summary>
        /// Automatically select the last used device when debugging.
        /// </summary>
        bool? AutoSelectLastUsedDevice { get; set; }

        /// <summary>
        /// The last selected device.
        /// </summary>
        IDevice CurrentDevice { get; set; }

        /// <summary>
        /// Process ID of the process that is being debugged.
        /// </summary>
        int CurrentProcessId { get; set; }

        /// <summary>
        /// Create a new selection container
        /// </summary>
        IIdeSelectionContainer CreateSelectionContainer(IServiceProvider serviceProvider);

        /// <summary>
        /// Gets a type map for the given android version.
        /// </summary>
        FrameworkTypeMap GetFrameworkTypeMap(int apiLevel);
    }
}
