using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Dot42.Gui.Controls;
using Dot42.Gui.Controls.Home;
using Dot42.Gui.Properties;
using Dot42.Gui.SamplesTool;
using Dot42.Shared.UI;
using Dot42.Utility;

namespace Dot42.Gui.Forms.Android
{
    public partial class MainForm : Form, IModalPanelForm
    {
        private IModalPanelForm progressForm;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            statusBar.AccessibleRole = AccessibleRole.StatusBar;

            devicesControl.Visible = true;
            devicesControl.Dock = DockStyle.Fill;
            //buttonGetStarted.Command = startViewControl.BrowseGettingStarted;
            //buttonOpenSamples.Command = startViewControl.OpenSamplesFolder;

            devicesControl.Visible = false;
            devicesControl.Dock = DockStyle.Fill;
            buttonInstallApk.Click += devicesControl.OnInstallApkClick;
            // buttonStartActivity.Click += devicesControl.StartActivity;
            buttonLogCat.Click += devicesControl.OnShowLogCatExecuted;
            buttonConnectDevice.Click += devicesControl.OnConnectNetworkedDevice;
            buttonRefresh.Click += devicesControl.OnRefreshDevices;

            lbVersion.Text = String.Format("dot42 C# for Android {0}", GetType().Assembly.GetName().Version.ToString());

            ResumeLayout(true);
            UserPreferences.Preferences.AttachMainWindow(this, new Size(800, 400));
            devicesControl.Visible = true;
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
        /// Visit Google play publish site.
        /// </summary>
        private void OnPublishAppClick(object sender, EventArgs e)
        {
            CommonActions.OpenUrl(URLConstants.GOOGLE_PLAY_PUBLISH);
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
        /// Open the samples folder
        /// </summary>
        private void OnOpenSampleFolderExecuted(object sender, EventArgs e)
        {

        }
    }
}
