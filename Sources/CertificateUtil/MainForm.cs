using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Dot42.CryptoUI;

namespace Dot42.CertificateUtil
{
    public partial class MainForm : Form
    {
        private X509Certificate2 certificate;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            UpdateState();
        }

        /// <summary>
        /// Open a pfx
        /// </summary>
        private void OnOpenClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Certificates|*.pfx";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Open(dialog.FileName, null);
                }
            }
        }

        /// <summary>
        /// Open a certificate
        /// </summary>
        private void Open(string path, string password)
        {
            certificate = null;
            try
            {
                certificate = new X509Certificate2(path, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                tbInfo.Text = certificate.Thumbprint;
            }
            catch
            {
                if (password == null)
                {
                    // Password needed
                    using (var dialog = new PasswordDialog())
                    {
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                            Open(path, dialog.Password);
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect password");
                }
            }
            finally
            {
                UpdateState();
            }
        }

        /// <summary>
        /// Import certificate in personal store
        /// </summary>
        private void OnImportClick(object sender, EventArgs e)
        {
            if (certificate == null)
                return;

            var x509Store2 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                x509Store2.Open(OpenFlags.ReadWrite);
                x509Store2.Add(certificate);
                x509Store2.Close();
                MessageBox.Show("Certificate successfully imported into the certificate store.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to import because: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Update controls
        /// </summary>
        private void UpdateState()
        {
            cmdImport.Enabled = (certificate != null);
        }

        /// <summary>
        /// Create new certificate
        /// </summary>
        private void OnCreateClick(object sender, EventArgs e)
        {
            using (var dialog = new CertificateWizard())
            {
                dialog.ShowDialog(this);
            }
        }
    }
}
