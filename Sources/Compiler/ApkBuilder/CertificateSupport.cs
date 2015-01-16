using System.Security.Cryptography.X509Certificates;

namespace Dot42.ApkBuilder
{
    internal static class CertificateSupport
    {
        /// <summary>
        /// Load a certificate by its thumbprint.
        /// </summary>
        internal static X509Certificate2 GetCertByThumbprint(string thumbprint)
        {
            var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                x509Store.Open(OpenFlags.ReadOnly);
                var x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (x509Certificate2Collection.Count == 1)
                {
                    return x509Certificate2Collection[0];
                }
            }
            finally
            {
                x509Store.Close();
            }
            return null;
        }

    }
}
