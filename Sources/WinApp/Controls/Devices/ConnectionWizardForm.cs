using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro.ColorTables;
using Dot42.DeviceLib.UI;
using Dot42.Graphics;
using Dot42.Gui.Licensing;
using Dot42.Gui.Properties;
using Dot42.Licensing;
using Dot42.Shared.UI;

namespace Dot42.Gui.Controls.Devices
{
    /// <summary>
    /// Wizard that helps the user to connect his android device.
    /// </summary>
    public partial class ConnectionWizardForm : AppForm
    {
        private bool wifiPossible;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConnectionWizardForm()
        {
            InitializeComponent();
            OnWifiOptionChanged(this, EventArgs.Empty);
            cbManufacturer.Items.AddRange(ManufacturerInfo.Load().ToArray());
            cbManufacturer.Items.Add(new ListViewItem(Resources.ManufacturerNotListed));
            OnManufacturerSelectedIndexChanged(this, EventArgs.Empty);
            OnCannotConnectionOptionChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// A settings in the connect over wifi page has changed.
        /// </summary>
        private void OnWifiOptionChanged(object sender, EventArgs e)
        {
            wifiPossible = false;
            if (switchButtonWifi.Value && switchButtonIsRooted.Value)
            {
                // WIFI can be Possible
                if (switchButtonAdbWifiInstalled.Value)
                {
                    wifiPossible = true;
                    lbWifiResult.Text = Resources.WifiConnectionPossible;
                }
                else
                {
                    // Install APP first
                    lbWifiResult.Text = string.Format(Resources.WifiConnectionPossibleInstallAppXUsingUrlY, "ADB WIFI", Urls.AdbWifi);
                }
            }
            else
            {
                // WIFI is not possible
                lbWifiResult.Text = Resources.WifiConnectionNotPossible;
            }
            cmdConnectOverWifi.Visible = wifiPossible;
            connectOverWifiPage.NextButtonEnabled = eWizardButtonState.True;
            connectOverWifiPage.NextButtonVisible = eWizardButtonState.True;
            connectOverWifiPage.FinishButtonEnabled = wifiPossible ? eWizardButtonState.True : eWizardButtonState.False;
        }

        /// <summary>
        /// Open browser on click
        /// </summary>
        private void OnMarkupLinkClick(object sender, MarkupLinkClickEventArgs e)
        {
            var href = e.HRef;
            if (href.StartsWith("http:") || href.StartsWith("https:") || href.StartsWith("ftp:"))
            {
                try
                {
                    Process.Start(href);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Show connect over wifi dialog
        /// </summary>
        private void OnConnectOverWifiClick(object sender, EventArgs e)
        {
            using (var dialog = new ConnectTcpipForm())
            {
                dialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Close on Finish
        /// </summary>
        private void OnFinishButtonClick(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Gets the selected manufacturer listviewitem.
        /// </summary>
        private object SelectedManufacturer
        {
            get
            {
                var selection = cbManufacturer.SelectedItems;
                return (selection.Count > 0) ? selection[0] : null;
            }
        }

        /// <summary>
        /// Wizard is changing to another page.
        /// </summary>
        private void OnWizardPageChanging(object sender, WizardCancelPageChangeEventArgs e)
        {
            if ((e.OldPage == connectionTypePage) && (e.PageChangeSource == eWizardPageChangeSource.NextButton))
            {
                if (cbConnectViaUsb.Checked)
                    e.NewPage = enableUsbDebuggingPage;
            }
            if (e.NewPage != null)
            {
                e.NewPage.BackColor = Color.White;
            }
            if (e.NewPage == manufacturerInfoPage)
            {
                // Is a manufacturer set?
                var manufacturer = SelectedManufacturer as ManufacturerInfo;
                if (manufacturer == null)
                {
                    // Cancel
                    e.Cancel = true;

                    if (e.PageChangeSource == eWizardPageChangeSource.NextButton)
                    {
                        // Go to "cannot connect" instead
                        wizard.SelectedPage = cannotConnectPage;
                    }
                    else if (e.PageChangeSource == eWizardPageChangeSource.BackButton)
                    {
                        // Go to "select manufacturer" page.
                        wizard.SelectedPage = manufacturerSelectionPage;
                    }
                }
            }
        }

        /// <summary>
        /// Manufacturer has changed
        /// </summary>
        private void OnManufacturerSelectedIndexChanged(object sender, EventArgs e)
        {
            var manufacturer = SelectedManufacturer as ManufacturerInfo;
            manufacturerSelectionPage.NextButtonEnabled = (SelectedManufacturer != null)
                                                              ? eWizardButtonState.True
                                                              : eWizardButtonState.False;
            if (manufacturer != null)
            {
                lbManufacturerHelp.Text = manufacturer.HelpInfo;
                manufacturerInfoPage.PageTitle = manufacturer.Name;
            }
        }

        /// <summary>
        /// Value has changed.
        /// </summary>
        private void OnCannotConnectionOptionChanged(object sender, EventArgs e)
        {
            var hasName = !string.IsNullOrEmpty(tbName.Text);
            var hasEmail = !string.IsNullOrEmpty(tbEmail.Text);
            var hasDevice = !string.IsNullOrEmpty(tbDevice.Text);

            cmdSendToSupport.Enabled = hasName && hasEmail && hasDevice;
        }

        /// <summary>
        /// Send help request to support.
        /// </summary>
        private void OnSendToSupportClick(object sender, EventArgs e)
        {
            var email = tbEmail.Text;
            var msg = CreateSupportMessage();
            sendToSupportProgress.Visible = true;
            sendToSupportProgress.IsRunning = true;
            cmdSendToSupport.Enabled = false;

            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task = Task.Factory.StartNew(() => ReportMessageToSupport(msg, email));
            task.ContinueWith(t => {
                sendToSupportProgress.IsRunning = false;
                sendToSupportProgress.Visible = false;
                if (t.Result)
                {
                    MessageBox.Show(Resources.SupportRequestSend, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(Resources.SupportRequestFailed, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                cmdSendToSupport.Enabled = true;
                Close();
            }, ui);
        }

        /// <summary>
        /// Create the support message text.
        /// </summary>
        private string CreateSupportMessage()
        {
            var name = tbName.Text;
            var email = tbEmail.Text;
            var device = tbDevice.Text;

            var nl = Environment.NewLine;
            var stringBuilder = new StringBuilder();

            try
            {
                stringBuilder.Append(nl);
                stringBuilder.AppendFormat("Product name: {0}{1}", Application.ProductName, nl);
                stringBuilder.AppendFormat("Product version: {0}{1}", Application.ProductVersion, nl);
                stringBuilder.AppendFormat("Serial: {0}{1}", Helper.GetSerial(), nl);
                stringBuilder.Append(nl);
            }
            catch
            {
                // Ignore
            }

            stringBuilder.AppendFormat("Name: {0}{1}", name, nl);
            stringBuilder.AppendFormat("Email: {0}{1}", email, nl);
            var manufacturer = SelectedManufacturer as ManufacturerInfo;
            stringBuilder.AppendFormat("Manufacturer: {0}{1}", (manufacturer != null) ? manufacturer.Name : "?", nl);
            stringBuilder.AppendFormat("Device: {0}{1}", device, nl);
            stringBuilder.AppendFormat("WIFI possible: {0}{1}", wifiPossible ? "Yes" : "No", nl);
            stringBuilder.Append(nl);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Cannot connect page has been displayed.
        /// </summary>
        private void OnCannotConnectPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            tbName.Focus();
        }

        /// <summary>
        /// Select manfacturer page has been displayed.
        /// </summary>
        private void OnManufacturerSelectionPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            cbManufacturer.Focus();
        }

        /// <summary>
        /// Connect over WIFI page has been displayed.
        /// </summary>
        private void OnConnectOverWifiPageDisplayed(object sender, WizardPageChangeEventArgs e)
        {
            switchButtonWifi.Focus();
        }

        /// <summary>
        /// Report problems to support.
        /// </summary>
        private static bool ReportMessageToSupport(string errorMessage, string email)
        {
            try
            {
                HttpStatusCode queryResult = HttpStatusCode.Created;

                // Prepare post data (i.e. Request.Form)
                string postData = String.Format(
                    "Message={0}&Subject={1}&NotificationOnly=1&Email={2}",
                    HttpUtility.UrlEncode(errorMessage),
                    HttpUtility.UrlEncode("Dot42 Device Connection Support Request"),
                    HttpUtility.UrlEncode(email));
                Debug.WriteLine(string.Format("PostData: {0}", postData));

                var encoding = new UTF8Encoding();
                var postBytes = encoding.GetBytes(postData);

                //Debug.WriteLine(string.Format("Creating request to {0}", NotifyURL));

                // Prepare request
                var request = (HttpWebRequest)WebRequest.Create(Urls.NOTIFICATION_URL);
                //Debug.WriteLine(string.Format("Created request to {0}", NotifyURL));
                request.AllowAutoRedirect = false;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.KeepAlive = false;
                // Set the content length of the string being posted.
                request.ContentLength = postData.Length;

                //Debug.WriteLine(string.Format("Writing request to {0}", NotifyURL));

                try
                {
                    Stream postStream = request.GetRequestStream();
                    postStream.Write(postBytes, 0, postBytes.Length);
                    // Close the Stream object.
                    postStream.Close();

                    Debug.WriteLine("Getting response");

                    //Get the response
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    //Debug.WriteLine("Closing response");

                    response.Close();
                    queryResult = response.StatusCode;
                    //Debug.WriteLine("Done");

                    return true;
                }
                catch (WebException webEx)
                {
                    queryResult = ((HttpWebResponse)webEx.Response).StatusCode;
                }
            }
            catch
            {
                //Nothing to do...
            }
            return false;
        }

        /// <summary>
        /// Helper for manufacturer information
        /// </summary>
        private class ManufacturerInfo : ListViewItem
        {
            private readonly string name;
            private readonly string helpInfo;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ManufacturerInfo(string name, string helpInfo)
            {
                Text = name;
                this.name = name;
                this.helpInfo = helpInfo;
            }

            public string HelpInfo
            {
                get { return helpInfo; }
            }

            public string Name
            {
                get { return name; }
            }

            public override string ToString()
            {
                return name;
            }

            /// <summary>
            /// Load all known manufacturer info's
            /// </summary>
            public static IEnumerable<ManufacturerInfo> Load()
            {
                var doc = XDocument.Parse(Resources.UsbDriverInfo);
                foreach (var element in doc.Root.Elements("Manufacturer"))
                {
                    var name = element.Attribute("name");
                    if (name == null)
                        continue;                    
                    var reader = element.CreateReader();
                    reader.MoveToContent();
                    var info = reader.ReadInnerXml();
                    yield return new ManufacturerInfo(name.Value, info);
                }
            }
        }
    }
}
