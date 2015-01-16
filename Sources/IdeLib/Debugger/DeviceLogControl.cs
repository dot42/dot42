using Dot42.AdbLib;
using Dot42.DeviceLib.UI;

namespace Dot42.Ide.Debugger
{
    /// <summary>
    /// Specialized device log control
    /// </summary>
    public class DeviceLogControl : LogCatControl
    {
        private readonly IIde ide;
        private int pid = -1;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DeviceLogControl(IIde ide)
        {
            this.ide = ide;
            HideTextView();

            // Listen for events
            ide.CurrentDeviceChanged += OnDeviceChanged;
            ide.CurrentProcessIdChanged += OnProcessIdChanged;

            // Initialize
            OnProcessIdChanged(null, null);
            OnDeviceChanged(null, null);
        }

        /// <summary>
        /// Process ID of debugged process has changed.
        /// </summary>
        private void OnProcessIdChanged(object sender, System.EventArgs e)
        {
            pid = ide.CurrentProcessId;
        }

        /// <summary>
        /// Current device has changed.
        /// </summary>
        private void OnDeviceChanged(object sender, System.EventArgs e)
        {
            Run(ide.CurrentDevice);
        }

        /// <summary>
        /// Should the given entry be made visible?
        /// </summary>
        protected override bool Show(LogEntry entry)
        {
            if (entry.Pid != pid)
                return false;
            return base.Show(entry);
        }
    }
}
