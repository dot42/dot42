using System;
using System.Windows.Forms;
using Dot42.BarDeployLib;
using Dot42.DeviceLib.UI;
using Dot42.Utility;
using TallComponents.Common.Util;

namespace Dot42.Gui.Controls.Devices
{
    public partial class DevicesControl : Panel
    {
        private bool firstInstallApk = true;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DevicesControl()
        {
            InitializeComponent();
            devicesListView.IsCompatibleCheck = x => true;
        }

        /// <summary>
        /// Selection changed.
        /// </summary>
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlState();
        }

        /// <summary>
        /// Update the state of all controls
        /// </summary>
        private void UpdateControlState()
        {
            var selection = SelectedDevice;
        }

        /// <summary>
        /// Gets the selected Device or null if there is no selection.
        /// </summary>
        private DeviceItem SelectedDevice
        {
            get { return devicesListView.SelectedDevice; }
        }

        /// <summary>
        /// Try to install an APK of the selected device.
        /// </summary>
        internal void OnInstallApkClick(object sender, EventArgs e)
        {
            try
            {
                var selection = SelectedDevice;
                if (selection == null)
                    return;
                using (var dialog = new OpenFileDialog())
                {
                    if (firstInstallApk && Locations.SamplesFolderIsPossible)
                    {
                        dialog.InitialDirectory = Locations.SamplesFolder(Program.Target);
                    }
                    firstInstallApk = false;
                    dialog.Filter = "APK files|*.apk";
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        var apkPath = dialog.FileName;
                        var installApkControl = new InstallApkControl(selection.Device, apkPath);
                        var form = (IModalPanelForm) this.FindForm();
                        installApkControl.Done += (s, x) => form.CloseModalPanel();
                        form.ShowModalPanel(installApkControl, x => x.Start());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
            }
        }

        /// <summary>
        /// Show the connect networked device dialog.
        /// </summary>
        internal void OnConnectNetworkedDevice(object sender, EventArgs e)
        {
            using (var dialog = new ConnectTcpipForm())
            {
                dialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Open logcat form for selected device.
        /// </summary>
        internal void OnShowLogCatExecuted(object sender, EventArgs e)
        {
            var selection = SelectedDevice;
            if (selection == null)
                return;
            var form = new LogCatForm(selection.Device);
            form.Show(this);
        }

        /// <summary>
        /// Add a blackberry device.
        /// </summary>
        private void OnAddBlackBerryDevice(object sender, EventArgs e)
        {
            using (var dialog = new ConnectBlackBerryForm())
            {
                dialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Remove the selected black berry device.
        /// </summary>
        private void OnRemoveBlackBerryDevice(object sender, EventArgs e)
        {
            var selection = devicesListView.SelectedDevice as BlackBerryDeviceItem;
            if (selection == null)
                return;
            BlackBerryDevices.Instance.Remove((BlackBerryDevice) selection.Device);
        }

        /// <summary>
        /// Refresh the devices list.
        /// </summary>
        internal void OnRefreshDevices(object sender, EventArgs e)
        {
            devicesListView.RefreshDevices();            
        }
    }
}
