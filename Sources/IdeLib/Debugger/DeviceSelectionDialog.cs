using System;
using System.ComponentModel;
using System.Windows.Forms;
using Dot42.AdbLib;
using Dot42.DeviceLib.UI;
using Dot42.FrameworkDefinitions;
using Dot42.Utility;

namespace Dot42.Ide.Debugger
{
    /// <summary>
    /// Dialog used to select a device to debug on.
    /// </summary>
    public partial class DeviceSelectionDialog : Form
    {
        private readonly IIde package;
        private readonly string targetVersion;
        private bool useFromNowOnIsVisible;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DeviceSelectionDialog(IIde package, Func<DeviceItem, bool> isCompatible, string targetVersion)
        {
            Icon = Graphics.Icons.App;
            this.package = package;
            this.targetVersion = targetVersion;
            InitializeComponent();
            tbbConnectDevice.Image = Graphics.Icons32.Antenna;
            tbbRefresh.Image = Graphics.Icons32.Refresh;
            devicesListView.IsCompatibleCheck = isCompatible;
            devicesListView.DeviceRemoved += (s, x) => UpdateState();
            devicesListView.DeviceAdded += OnDeviceAdded;
            devicesListView.ItemActivated += OnItemActivated;
            UpdateState();
        }

        /// <summary>
        /// New target version of the project.
        /// </summary>
        public string NewProjectTargetVersion { get; private set; }

        /// <summary>
        /// Load devices on startup.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            devicesListView.DeviceMonitorEnabled = true;
        }

        /// <summary>
        /// Stop device monitor
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop monitor
            devicesListView.DeviceMonitorEnabled = false;

            // Save settings
            if (useFromNowOnIsVisible && (DialogResult == DialogResult.OK))
            {
                package.AutoSelectLastUsedDevice = cbUseFromNowOn.Checked;
            }
            
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Select device if needed.
        /// </summary>
        private void OnDeviceAdded(object sender, EventArgs<AndroidDevice> e)
        {
            lbLoadingDevices.Visible = false;
            if (SelectedDevice == null)
            {
                if (e.Data.Serial == package.LastUsedUniqueId)
                {
                    devicesListView.Select(e.Data);
                }
            }
            if (!cbUseFromNowOn.Visible)
            {
                useFromNowOnIsVisible = true;
                cbUseFromNowOn.Visible = true;
                if (package.AutoSelectLastUsedDevice.HasValue)
                {
                    cbUseFromNowOn.Checked = package.AutoSelectLastUsedDevice.Value;
                }
            }
            UpdateState();
            AutoSelectIfPossible();
        }

        /// <summary>
        /// Automatically select the device when requested to do so.
        /// </summary>
        private void AutoSelectIfPossible()
        {
            if (package.AutoSelectLastUsedDevice.HasValue && package.AutoSelectLastUsedDevice.Value)
            {
                var selection = SelectedDevice;
                if ((selection != null) && (selection.Serial == package.LastUsedUniqueId))
                {
                    OnSelectClick(this, EventArgs.Empty);
                }
            }            
        }

        /// <summary>
        /// Gets the selected AVD.
        /// </summary>
        public DeviceItem SelectedDevice
        {
            get { return devicesListView.SelectedDevice; }
        }

        /// <summary>
        /// Device selection has changed.
        /// </summary>
        private void OnSelectedDeviceChanged(object sender, EventArgs e)
        {
            UpdateState();
            AutoSelectIfPossible();
        }

        /// <summary>
        /// Update the state of the controls.
        /// </summary>
        private void UpdateState()
        {
            var selection = SelectedDevice;
            cmdOk.Enabled = (selection != null) && selection.IsConnected && selection.IsCompatible;
            var invalidVersion = (selection != null) && selection.IsConnected && !selection.IsCompatible;
            var canChangeProjectVersion = invalidVersion && !string.IsNullOrEmpty(targetVersion);
            if (invalidVersion)
            {
                lbLoadingDevices.Visible = false;
                var sdkVersion = selection.Device.SdkVersion;
                var deviceFramework = Frameworks.Instance.GetBySdkVersion(sdkVersion);
                if (deviceFramework == null)
                    canChangeProjectVersion = false;
                NewProjectTargetVersion = (deviceFramework != null) ? deviceFramework.Name : null;
                lbInvalidVersion.Text = canChangeProjectVersion ?
                    string.Format("Your project is targeting Android {0}, which is not supported by {1}.\nDo you want to change the target of your project to {2}?", 
                    targetVersion, selection.Device.Name, deviceFramework.Name) : "Your project is targeting an Android version that is not supported by this device";
            }
            lbInvalidVersion.Visible = invalidVersion;
            cmdChangeProjectVersion.Visible = canChangeProjectVersion;
        }

        /// <summary>
        /// Item in device list activated.
        /// </summary>
        private void OnItemActivated(object sender, EventArgs<DeviceItem> eventArgs)
        {
            var selection = eventArgs.Data;
            if (selection.IsConnected && selection.IsCompatible)
            {
                // OK click
                OnSelectClick(this, EventArgs.Empty);
            }
            else if (selection.CanStart)
            {
                // Start
                OnStartClick(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Select menu item clicked
        /// </summary>
        private void OnSelectClick(object sender, EventArgs e)
        {
            var selection = SelectedDevice;
            if (selection.IsConnected && selection.IsCompatible)
            {
                // OK click
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Start menu item clicked.
        /// </summary>
        private void OnStartClick(object sender, EventArgs e)
        {
            var selection = SelectedDevice;
            if ((selection == null) || !selection.CanStart)
                return;
            // Try to start the device.
            try
            {
                selection.Start(x => { });
            }
            catch (Exception ex)
            {
                var msg = string.Format("Failed to start {0} because: {1}.", selection.Name, ex.Message);
                MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update context menu on opening
        /// </summary>
        private void OnContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            var selection = SelectedDevice;
            miSelect.Visible = (selection != null) && selection.IsConnected;
            miStart.Visible = (selection != null) && selection.CanStart;

            // Cancel if there are no options
            if ((selection == null) || (!(selection.IsConnected || selection.CanStart)))
                e.Cancel = true;
        }

        /// <summary>
        /// Listen for blackberry notifications
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            BlackBerryNotifications.FilterUpdateNotification(ref m);
            base.WndProc(ref m);
        }

        /// <summary>
        /// Refresh devices.
        /// </summary>
        private void OnRefreshClick(object sender, EventArgs e)
        {
            devicesListView.RefreshDevices();
        }

        /// <summary>
        /// Connect networked device
        /// </summary>
        private void OnConnectDeviceClick(object sender, EventArgs e)
        {
            using (var dialog = new ConnectTcpipForm())
            {
                dialog.ShowDialog(this);
            }
        }
    }
}
