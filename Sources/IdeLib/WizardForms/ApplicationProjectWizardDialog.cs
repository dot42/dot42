using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dot42.CryptoUI;
using Dot42.FrameworkDefinitions;
using Dot42.Shared.UI;

namespace Dot42.Ide.WizardForms
{
    public partial class ApplicationProjectWizardDialog : Form
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ApplicationProjectWizardDialog()
        {
            InitializeComponent();
            UpdateState();
            var frameworkNames = Frameworks.Instance.OrderByDescending(x => x.Name).Select(x => x.Name).ToArray();
            cbFramework.Items.AddRange(frameworkNames);
            if (cbFramework.Items.Count > 0)
                cbFramework.SelectedIndex = 0;

            // Restore preferences
            var prefs = UserPreferences.Preferences;
            var lastVersion = prefs.FrameworkVersion;
            if (!string.IsNullOrEmpty(lastVersion))
            {
                var index = Array.IndexOf(frameworkNames, lastVersion);
                if (index >= 0)
                    cbFramework.SelectedIndex = index;
            }
            if (!string.IsNullOrEmpty(prefs.CertificatePath) && !string.IsNullOrEmpty(prefs.CertificateThumbPrint))
            {
                CertificatePath = prefs.CertificatePath;
                CertificateThumbprint = prefs.CertificateThumbPrint;
                UpdateCertificateText();
            }

            UpdateState();
        }

        /// <summary>
        /// Full path of certificate
        /// </summary>
        public string CertificatePath { get; private set; }

        /// <summary>
        /// Thumb print of certificate
        /// </summary>
        public string CertificateThumbprint { get; private set; }

        /// <summary>
        /// Version of target framework
        /// </summary>
        public string TargetFrameworkVersion
        {
            get { return (string) cbFramework.SelectedItem; }
        }

        /// <summary>
        /// The names of the selected additional DLL's
        /// </summary>
        public string[] AdditionalLibraryNames
        {
            get { return additionalLibrariesControl1.CheckedLibraries.Select(x => x.DllName).ToArray(); }
        }

        /// <summary>
        /// Select a certificate
        /// </summary>
        private void cmdOpenCertificate_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = ".pfx";
                dialog.Filter = "Certificates|*.pfx|All files|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenCertificate(dialog.FileName, null);
                }
            }
        }

        /// <summary>
        /// Create a new certificate.
        /// </summary>
        private void OnCreateCertificateClick(object sender, EventArgs e)
        {
            using (var dialog = new CertificateWizard())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                OpenCertificate(dialog.Path, dialog.Password);
            }
        }

        /// <summary>
        /// Open a certificate file.
        /// </summary>
        private void OpenCertificate(string path, string password)
        {
            try
            {
                string certificateThumbprint;
                if (CertificateHelper.OpenCertificate(path, password, out certificateThumbprint))
                {
                    CertificateThumbprint = certificateThumbprint;
                    CertificatePath = path;
                    UpdateCertificateText();
                }
            }
            finally
            {
                UpdateState();
            }

        }

        /// <summary>
        /// Update the UI for the selected certificate.
        /// </summary>
        private void UpdateCertificateText()
        {
            var path = CertificatePath;
            tbCertificatePath.Text = string.IsNullOrEmpty(path) ? "" : Path.GetFileNameWithoutExtension(path);            
        }

        /// <summary>
        /// Update control state
        /// </summary>
        private void UpdateState()
        {
            cmdOK.Enabled = !string.IsNullOrEmpty(CertificatePath) && !string.IsNullOrEmpty(CertificateThumbprint) && !string.IsNullOrEmpty(TargetFrameworkVersion);
        }

        /// <summary>
        /// Target framework version has changed.
        /// </summary>
        private void OnTargetFrameworkVersionChanged(object sender, EventArgs e)
        {
            UpdateState();
        }

        /// <summary>
        /// Ok clicked.
        /// </summary>
        private void OnOkClick(object sender, EventArgs e)
        {
            // Accept all license agreements
            var additionalLibs = additionalLibrariesControl1.CheckedLibraries.ToList();
            if (!additionalLibs.AcceptToAll())
                return;

            // Save preferences
            var prefs = UserPreferences.Preferences;
            prefs.FrameworkVersion = TargetFrameworkVersion;
            prefs.CertificatePath = CertificatePath;
            prefs.CertificateThumbPrint = CertificateThumbprint;
            UserPreferences.SaveNow();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
