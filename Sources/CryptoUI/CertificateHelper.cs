using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Dot42.CryptoUI
{
    /// <summary>
    /// Static helpers for working with certificates.
    /// </summary>
    public static class CertificateHelper
    {

        /// <summary>
        /// Open a certificate file.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public static bool OpenCertificate(string path, string password, out string certificateThumbprint)
        {
            try
            {
                var certificate = new X509Certificate2(path, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                certificateThumbprint = certificate.Thumbprint;
                ImportCertificate(certificate);
                return true;
            }
            catch
            {
                // Password needed
                if (password == null)
                {
                    using (var dialog = new PasswordDialog())
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            return OpenCertificate(path, dialog.Password, out certificateThumbprint);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect password");
                }
                certificateThumbprint = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Import the given certificate into the current users store.
        /// </summary>
        private static void ImportCertificate(X509Certificate2 certificate)
        {
            // Import in store of current user
            var x509Store2 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                x509Store2.Open(OpenFlags.ReadWrite);
                x509Store2.Add(certificate);
                x509Store2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to import certificate into user store because: {0}", ex.Message));
            }
        }

    }
}
