using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dot42.CryptoUI.Controls;

namespace Dot42.CryptoUI
{
    public partial class CertificateWizard : Form, ICertificateSettings
    {
        private Button lastPressedButton = null;
        private static readonly char[] INVALID_PATH_CHARS =
            new char[] { '\\', ':', '/', '@', '%', '|' }.Concat(System.IO.Path.GetInvalidFileNameChars()).Concat(System.IO.Path.GetInvalidPathChars()).ToArray();
        private bool tabChangedByButton = false;

        /// <summary>
        /// Default ctor
        /// </summary>
        public CertificateWizard()
        {
            this.InitializeComponent();
            this.settingsControl.StateChanged += (s, x) => this.UpdateState();
            this.passwordControl.StateChanged += (s, x) => this.UpdateState();
            this.saveAsControl.StateChanged += (s, x) => this.UpdateState();
        }

        /// <summary>
        /// Path of the generated certificate.
        /// </summary>
        public string Path { get { return this.saveAsControl.FileName; } }

        int ICertificateSettings.MaxYears { get { return int.MaxValue; } }

        bool ICertificateSettings.ImportInCertificateStore
        {
            get { return this.saveAsControl.ImportInCertificateStore; }
        }

        string ICertificateSettings.OrgName
        {
            get { return this.settingsControl.OrgName; }
        }

        string ICertificateSettings.OrgUnit
        {
            get { return this.settingsControl.OrgUnit; }
        }

        string ICertificateSettings.City
        {
            get { return this.settingsControl.City; }
        }

        string ICertificateSettings.CountryCode
        {
            get { return this.settingsControl.Country.Code; }
        }

        string ICertificateSettings.State
        {
            get { return this.settingsControl.State; }
        }

        string ICertificateSettings.Email
        {
            get { return this.settingsControl.Email; }
        }

        string ICertificateSettings.UserName
        {
            get { return this.settingsControl.UserName; }
        }

        /// <summary>
        /// Password of the generated certificate.
        /// </summary>
        public string Password { get { return passwordControl.Password; } }

        /// <summary>
        /// Update state after a new page is shown.
        /// </summary>
        private void OnWizardPageChanged(object sender, EventArgs e)
        {
            this.UpdateState();

            if (4 == this.wizard.SelectedIndex)
            {
                this.cancelButton.Text = "Finished";
            }
            else
            {
                this.cancelButton.Text = "Cancel";
            }

            switch (this.wizard.SelectedIndex)
            {
                case 1:
                    this.settingsControl.FocusFirst();
                    break;
                case 2:
                    this.passwordControl.FocusFirst();
                    break;
                case 3:
                    if (this.nextButton == this.lastPressedButton)
                    {
                        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var name = CertificateWizard.StripInvalidPathCharacters(this.settingsControl.UserName);
                        if (name.Length == 0) name = "Certificate";
                        this.saveAsControl.FileName = System.IO.Path.Combine(folder, name + ".pfx");
                    }
                    this.saveAsControl.FocusFirst();
                    break;
                case 4:
                    this.CreateCertificate();
                    break;
            }
        }

        /// <summary>
        /// Update state of the wizard buttons
        /// </summary>
        private void UpdateState()
        {
            ICertificateWizardPage page;
            switch (this.wizard.SelectedIndex)
            {
                case 1:
                    page = this.settingsControl;
                    this.nextButton.Enabled = !page.IsNextButtonDisabled;
                    this.backButton.Enabled = !page.IsBackButtonDisabled;
                    break;
                case 2:
                    page = this.passwordControl;
                    this.nextButton.Enabled = !page.IsNextButtonDisabled;
                    this.backButton.Enabled = !page.IsBackButtonDisabled;
                    break;
                case 3:
                    page = this.saveAsControl;
                    this.nextButton.Enabled = !page.IsNextButtonDisabled;
                    this.backButton.Enabled = !page.IsBackButtonDisabled;
                    break;
            }
        }

        
        /// <summary>
        /// Create the actual certificate
        /// </summary>
        private void CreateCertificate()
        {
            this.CreateCertificate(this.creatingCertificateControl.Log);
            this.nextButton.Enabled = false;
        }

        /// <summary>
        /// Create the actual certificate
        /// </summary>
        private void CreateCertificate(Action<string> log)
        {
            string errorMessage;
            string thumbprint;
            if (!CertificateBuilder.CreateCertificate(this, log, out thumbprint, out errorMessage))
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);                    
                }
            }
        }

        /// <summary>
        /// Remove all non-valid characters from the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string StripInvalidPathCharacters(string path)
        {
            int idx;
            while ((idx = path.IndexOfAny(INVALID_PATH_CHARS)) >= 0)
            {
                path = path.Substring(0, idx) + path.Substring(idx + 1);
            }
            return path;
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void BackButtonClicked(object sender, EventArgs e)
        {
            if (this.wizard.SelectedIndex > 0)
            {
                this.lastPressedButton = this.backButton;
                this.tabChangedByButton = true;
                this.wizard.SelectedIndex--;
            }
        }

        /// <summary>
        /// Next button clicked
        /// </summary>
        private void NextButtonClicked(object sender, EventArgs e)
        {
            if (this.wizard.SelectedIndex < this.wizard.TabCount - 1)
            {
                this.lastPressedButton = this.nextButton;
                this.tabChangedByButton = true;
                this.wizard.SelectedIndex++;
            }
        }
     
        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void CancelButtonClicked(object sender, EventArgs e)
        {

            DialogResult = this.cancelButton.Text.Equals("Cancel") ? DialogResult.Cancel : DialogResult.OK;
            this.Close();
        }

        private void AllowTabToChange(object sender, TabControlCancelEventArgs e)
        {
            if (this.tabChangedByButton)
            {
                this.tabChangedByButton = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
