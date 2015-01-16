using System;
using System.Net;
using System.Windows.Forms;
using Dot42.BarDeployLib;
using Dot42.Shared.UI;

namespace Dot42.DeviceLib.UI
{
    public partial class ConnectBlackBerryForm : AppForm
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ConnectBlackBerryForm()
        {
            InitializeComponent();
            cmdOK.Enabled = false;
        }

        /// <summary>
        /// Create BB Device
        /// </summary>
        private void OnOkClick(object sender, EventArgs e)
        {
            var address = tbAddress.Text;
            var password = tbPassword.Text;

            BlackBerryDevices.Instance.Add(address, password);
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Cancel.
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Text in IP/password field has changed.
        /// </summary>
        private void OnValueChanged(object sender, EventArgs e)
        {
            var address = tbAddress.Text;
            var password = tbPassword.Text;
            IPAddress ipAddress;
            cmdOK.Enabled = !string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(password) && IPAddress.TryParse(address, out ipAddress);
        }
    }
}
