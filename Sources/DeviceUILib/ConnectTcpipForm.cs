using System;
using System.Net;
using System.Windows.Forms;
using Dot42.Shared.UI;

namespace Dot42.DeviceLib.UI
{
    public partial class ConnectTcpipForm : Form
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ConnectTcpipForm()
        {
            InitializeComponent();
            cmdOK.Enabled = false;
        }

        /// <summary>
        /// Load time initialization
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            tbAddress.Text = UserPreferences.Preferences.DeviceConnectionAddress;
        }

        /// <summary>
        /// Create AVD
        /// </summary>
        private void OnOkClick(object sender, EventArgs e)
        {
            var address = tbAddress.Text;
            UserPreferences.Preferences.DeviceConnectionAddress = address;
            UserPreferences.SaveNow();

            var control = new ConnectingTcpipControl(address, tbPort.Value.ToString());
            control.Close += (s, x) => {
                modalPanelContainer.CloseModalPanel(control);
                DialogResult = DialogResult.OK;
            };
            control.Cancel += OnCancelClick;
            ClientSize = control.Size;
            modalPanelContainer.ShowModalPanel(control);
            control.Start();
            cmdOK.Enabled = false;
            cmdCancel.Enabled = false;
        }

        /// <summary>
        /// Cancel.
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Text in name field has changed.
        /// </summary>
        private void OnAddressChanged(object sender, EventArgs e)
        {
            var address = tbAddress.Text;
            IPAddress ipAddress;
            cmdOK.Enabled = !string.IsNullOrEmpty(address) && IPAddress.TryParse(address, out ipAddress);
        }
    }
}
