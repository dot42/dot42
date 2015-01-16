using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Dot42.CryptoUI.Controls;
using Dot42.Utility;
using Org.BouncyCastle.Asn1;
using TallComponents.Common.Util;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Dot42.CryptoUI
{
    /// <summary>
    /// Helper used to create certificates.
    /// </summary>
    public static class CertificateBuilder
    {
        /// <summary>
        /// Create the actual certificate
        /// </summary>
        public static bool CreateCertificate(ICertificateSettings settings, Action<string> log, out string thumbprint, out string errorMessage)
        {
            errorMessage = string.Empty;
            thumbprint = string.Empty;

            try
            {
                var keyStore = new Pkcs12Store();

                log(Strings.GeneratingKeys);
                var pGen = new RsaKeyPairGenerator();
                var genParam = new
                    RsaKeyGenerationParameters(
                    BigInteger.ValueOf(0x10001),
                    new SecureRandom(),
                    1024,
                    10);
                pGen.Init(genParam);
                var keyPair = pGen.GenerateKeyPair();

                log(Strings.GeneratingCertificate);
                var attrs = new Dictionary<DerObjectIdentifier, string>();
                var oids = new List<DerObjectIdentifier> { X509Name.O, X509Name.L, X509Name.C, X509Name.CN, X509Name.EmailAddress, X509Name.OU, X509Name.ST };
                oids.Reverse();
                if (!string.IsNullOrEmpty(settings.OrgName)) attrs.Add(X509Name.O, settings.OrgName); else oids.Remove(X509Name.O);
                if (!string.IsNullOrEmpty(settings.OrgUnit)) attrs.Add(X509Name.OU, settings.OrgUnit); else oids.Remove(X509Name.OU);
                if (!string.IsNullOrEmpty(settings.City)) attrs.Add(X509Name.L, settings.City); else oids.Remove(X509Name.L);
                if (!string.IsNullOrEmpty(settings.CountryCode)) attrs.Add(X509Name.C, settings.CountryCode); else oids.Remove(X509Name.C);
                if (!string.IsNullOrEmpty(settings.State)) attrs.Add(X509Name.ST, settings.State); else oids.Remove(X509Name.ST);
                if (!string.IsNullOrEmpty(settings.Email)) attrs.Add(X509Name.EmailAddress, settings.Email); else oids.Remove(X509Name.EmailAddress);
                if (!string.IsNullOrEmpty(settings.UserName)) attrs.Add(X509Name.CN, settings.UserName); else oids.Remove(X509Name.CN);

                var certGen = new X509V3CertificateGenerator();
                var random = new SecureRandom();

                certGen.SetSerialNumber(BigInteger.ValueOf(Math.Abs(random.NextInt())));
                certGen.SetIssuerDN(new X509Name(oids, attrs));
                certGen.SetNotBefore(DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)));
                var years = Math.Min(settings.MaxYears, 50);
                certGen.SetNotAfter(DateTime.Today.AddYears(years));
                certGen.SetSubjectDN(new X509Name(oids, attrs));
                certGen.SetPublicKey(keyPair.Public);
                certGen.SetSignatureAlgorithm("SHA1WithRSAEncryption");

                var cert = certGen.Generate(keyPair.Private);

                // Save
                log(Strings.SavingCertificate);
                var keyEntry = new AsymmetricKeyEntry(keyPair.Private);
                var certEntry = new X509CertificateEntry(cert);
                const string alias = "alias";
                keyStore.SetKeyEntry(alias, keyEntry, new[] { certEntry });

                var password = settings.Password;
                var memoryStream = new MemoryStream();
                keyStore.Save(memoryStream, password.ToCharArray(), random);
                memoryStream.Position = 0;

                // Save certificate
                var path = settings.Path;
                var folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder))
                    Directory.CreateDirectory(folder);

                // Set path in finished page.
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }

                if (settings.ImportInCertificateStore)
                {
                    log("Importing certificate in My Certificates store");
                    var certificate = new X509Certificate2(path, password,
                                                           X509KeyStorageFlags.Exportable |
                                                           X509KeyStorageFlags.PersistKeySet);
                    thumbprint = certificate.Thumbprint;
                    var x509Store2 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    try
                    {
                        x509Store2.Open(OpenFlags.ReadWrite);
                        x509Store2.Add(certificate);
                        x509Store2.Close();
                    }
                    catch (Exception ex)
                    {
                        errorMessage = string.Format("Failed to import certificate because: {0}", ex.Message);
                        return false;
                    }
                }

                if (years < 30)
                {
                    log("Certificate is intended for evaluation purposes. It cannot be used to deploy to the market.");
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.DumpError(ex);
                errorMessage = string.Format("Failed to create certificate because {0}.", ex.Message);
                return false;
            }
        }
    }
}
