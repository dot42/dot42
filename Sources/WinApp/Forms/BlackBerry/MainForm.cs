using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using Dot42.DeviceLib.UI;
using Dot42.Graphics;
using Dot42.Gui.Controls;
using Dot42.Gui.Controls.Home;
using Dot42.Gui.Licensing;
using Dot42.Gui.Properties;
using Dot42.Gui.SamplesTool;
using Dot42.Licensing;
using Dot42.Shared.UI;
using Dot42.Utility;
using TallApplications.Common.Licensing;
using TallApplications.Common.Update;

namespace Dot42.Gui.Forms.BlackBerry
{
    public partial class MainForm : RibbonForm, IModalPanelForm
    {
        private IModalPanelForm progressForm;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            styleManager1.Setup();
            statusBar.AccessibleRole = AccessibleRole.StatusBar;

            buttonWebsite.Image = Icons.icon_32x32_rgba;
            buttonOpenSamples.Image = Icons32.FolderView;
            buttonInstallApk.Image = Icons32.PackageAdd;
            buttonLogCat.Image = Icons32.History;
            buttonAddDevice.Image = Icons32.Antenna;
            buttonRemoveDevice.Image = Icons32.EmulatorDelete;
            //buttonNewVDevice.Image = Icons32.EmulatorNew;
            //buttonStartVDevice.Image = Icons32.EmulatorPlay;
            //buttonEditVDevice.Image = Icons16.EmulatorEdit;
            //buttonRemoveVDevice.Image = Icons32.EmulatorDelete;
            buttonActivate.Image = Icons32.Key;
            buttonCheckForUpdates.Image = Icons32.Download;

            devicesControl.Visible = true;
            devicesControl.Dock = DockStyle.Fill;
            //buttonGetStarted.Command = startViewControl.BrowseGettingStarted;
            //buttonOpenSamples.Command = startViewControl.OpenSamplesFolder;

            devicesControl.Visible = false;
            devicesControl.Dock = DockStyle.Fill;
            buttonInstallApk.Command = devicesControl.InstallApk;
            buttonStartActivity.Command = devicesControl.StartActivity;
            buttonLogCat.Command = devicesControl.LogCat;
            buttonAddDevice.Command = devicesControl.AddBlackBerryDevice;
            buttonRemoveDevice.Command = devicesControl.RemoveBlackBerryDevice;
            //buttonNewVDevice.Command = devicesControl.NewEmulator;
            //buttonEditVDevice.Command = devicesControl.EditEmulator;
            //buttonRemoveVDevice.Command = devicesControl.RemoveDevice;
            //buttonStartVDevice.Command = devicesControl.StartEmulator;

            lbVersion.Text = String.Format("dot42 C# for BlackBerry {0}", GetType().Assembly.GetName().Version.ToString());
            UpdateLicensingInfo();

            ResumeLayout(true);
            UserPreferences.Preferences.AttachMainWindow(this, new Size(800, 400));
            Application.Idle += OnApplicationIdle;
            devicesControl.Visible = true;
            ribbon.SelectedRibbonTabItem = tabItemDevices;
        }

        /// <summary>
        /// Load time initialization
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            buttonOpenSamples.Enabled = Locations.SamplesFolderIsPossible;
        }

        /// <summary>
        /// Show activate dialog the first time the application is idle.
        /// </summary>
        void OnApplicationIdle(object sender, EventArgs e)
        {
            // Disconnect
            Application.Idle -= OnApplicationIdle;

            if (!(LicenseContainer.IsValidBlackBerryEvalLicense()  || LicenseContainer.IsValidBlackBerryProLicense()))
            {
                Activate(); 
            }
        }

        /// <summary>
        /// Show the given control as modal panel
        /// </summary>
        void IModalPanelForm.ShowModalPanel<T>(T createControl, Action<T> initialize)
        {
            var oldProgressForm = progressForm;
            try
            {
                using (var dialog = new ProgressForm<T>(createControl, initialize))
                {
                    progressForm = dialog;
                    dialog.ShowDialog(this);
                }
            }
            finally
            {
                progressForm = oldProgressForm;
            }
        }

        /// <summary>
        /// Close the current modal panel
        /// </summary>
        void IModalPanelForm.CloseModalPanel()
        {
            var dialog = progressForm;
            if (dialog != null) 
                dialog.CloseModalPanel();
        }

        /// <summary>
        /// Try to activate the app
        /// </summary>
        internal void Activate()
        {
#if LICENSED
            if (Helper.Activation(ParentForm, LicenseContainer.GetBlackBerryLicense()))
            {
                UpdateLicensingInfo();
            }
#else
            // Keep this as literal string.
            MessageBox.Show("Download the licensed version from the My Account section of our website.");
#endif
        }

        /// <summary>
        /// Refresh license info
        /// </summary>
        private void UpdateLicensingInfo()
        {
            var license = LicenseContainer.GetBlackBerryLicense();
            if (license.LicenseState != LicenseState.NotFound)
            {
                buttonActivate.Text = "Re-activate";
                lbLicStatus.Text = String.Format("Licensed on: {0}, serial {1} ({2})", license.RuntimeAttributesKey, license.GetSerial(), GetLicenseType(license));
            }
            else
            {
                buttonActivate.Text = "Activate";
                lbLicStatus.Text = String.Format("User account: {0}", license.RuntimeAttributesKey);
            }
        }

        /// <summary>
        /// Gets the type of license currently running.
        /// </summary>
        private static string GetLicenseType(License license)
        {
            if (license == null)
                return "?";
            if (LicenseContainer.IsValidBlackBerryProLicense())
                return "Professional";
            if (LicenseContainer.IsValidBlackBerryEvalLicense())
                return "Evaluation";
            return "?";
        }

        /// <summary>
        /// Open the samples folder
        /// </summary>
        private void OnOpenSampleFolderExecuted(object sender, EventArgs e)
        {
            var folder = Locations.SamplesFolder(Targets.BlackBerry);
            if (!String.IsNullOrEmpty(folder))
            {
                try
                {
                    if (!Directory.Exists(folder))
                    {
                        using (var dialog = new SamplesToolForm(folder))
                        {
                            dialog.ShowDialog(this);
                        }
                    }
                    Process.Start(folder);
                }
                catch (Exception ex)
                {
                    var msg = String.Format(Resources.FailedToOpenSamplesFolderBecauseX, ex.Message);
                    MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Open dot42 homepage page.
        /// </summary>
        private void OnOpenWebsite(object sender, EventArgs e)
        {
            CommonActions.OpenUrl(URLConstants.HOME);
        }

        /// <summary>
        /// Start check for updates
        /// </summary>
        private void OnCheckForUpdatesClick(object sender, EventArgs e)
        {
            UpdateApp.RunUpdateApp(false, "dot42 Updater");
        }
        
        /// <summary>
        /// Activate/re-activate license
        /// </summary>
        private void OnActivateClick(object sender, EventArgs e)
        {
            Activate();
        }

        /// <summary>
        /// Catch notification messages
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            BlackBerryNotifications.FilterUpdateNotification(ref m);
            base.WndProc(ref m);
        }
    }
}
