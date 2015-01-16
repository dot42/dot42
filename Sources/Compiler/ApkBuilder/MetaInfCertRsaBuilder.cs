using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

namespace Dot42.ApkBuilder
{
    /// <summary>
    /// Builder of the META-INF/CERT.RSA file
    /// </summary>
    internal class MetaInfCertRsaBuilder
    {
        private class Oids
        {
            internal const string data = "1.2.840.113549.1.7.1";
            internal const string signedData = "1.2.840.113549.1.7.2";
            internal static readonly DerObjectIdentifier SHA1 = X509ObjectIdentifiers.IdSha1;
            internal static readonly DerObjectIdentifier MD5 = PkcsObjectIdentifiers.MD5;
            internal static readonly DerObjectIdentifier RSA = PkcsObjectIdentifiers.RsaEncryption;
            internal static readonly DerObjectIdentifier RSA_MD5 = PkcsObjectIdentifiers.MD5WithRsaEncryption;
        }

        private readonly MetaInfCertSfBuilder signature;
        private readonly string pfxFile;
        private readonly string pfxPassword;
        private readonly string certificateThumbprint;
        private readonly string freeAppsKey;
        private readonly string apkPackageName;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MetaInfCertRsaBuilder(MetaInfCertSfBuilder signature, string pfxFile, string pfxPassword, string certificateThumbprint, string freeAppsKey, string apkPackageName)
        {
            this.signature = signature;
            this.pfxFile = pfxFile;
            this.pfxPassword = pfxPassword;
            this.certificateThumbprint = certificateThumbprint;
            this.freeAppsKey = freeAppsKey;
            this.apkPackageName = apkPackageName;
        }

        /// <summary>
        /// Write myself to the given stream
        /// </summary>
        public void WriteTo(Stream stream, out string md5FingerPrint, out string sha1FingerPrint)
        {
            X509Certificate[] cert;
            AsymmetricKeyEntry privateKey;
            LoadPfx(out cert, out privateKey);

            var certsVector = new Asn1EncodableVector();
            md5FingerPrint = null;
            sha1FingerPrint = null;
            foreach (var c in cert)
            {
                var certStream = new MemoryStream(c.GetEncoded());
                var certStruct = X509CertificateStructure.GetInstance(new Asn1InputStream(certStream).ReadObject());
                certsVector.Add(certStruct);

                if (md5FingerPrint == null)
                {
                    var certData = certStream.ToArray();
                    md5FingerPrint = CreateFingerprint(new MD5Digest(), certData);
                }

                if (sha1FingerPrint == null)
                {
                    var certData = certStream.ToArray();
                    sha1FingerPrint = CreateFingerprint(new Sha1Digest(), certData);
                }
            }

            var encryptedSignature = GetSignature(signature, privateKey.Key);
            var signerInfo = new SignerInfo(
                new DerInteger(1),
                new IssuerAndSerialNumber(cert[0].IssuerDN, cert[0].SerialNumber),
                new AlgorithmIdentifier(Oids.SHA1, DerNull.Instance),
                null,
                new AlgorithmIdentifier(Oids.RSA, DerNull.Instance),
                new DerOctetString(encryptedSignature),
                null);

            var pkcs7 = new SignedData(
                new DerInteger(1),
                new DerSet(new AlgorithmIdentifier(Oids.SHA1, DerNull.Instance)),
                new ContentInfo(new DerObjectIdentifier(Oids.data), null),
                new DerSet(certsVector),
                null,
                new DerSet(signerInfo));

            //var signedData = new ContentInfo(new DERObjectIdentifier(Oids.signedData), pkcs7);

            var v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(Oids.signedData));
            v.Add(new DerTaggedObject(0, pkcs7));            
            var signedData = new DerSequence(v);

            // Save
            var data = signedData.GetEncoded();
            stream.Write(data, 0, data.Length);
        }

        private static byte[] GetSignature(MetaInfCertSfBuilder signature, ICipherParameters privateKey)
        {
            var signer = new RsaDigestSigner(new Sha1Digest());
            //var signer = new RSADigestSigner(new MD5Digest());
            signer.Init(true, privateKey);
            var raw = signature.ToArray();
            signer.BlockUpdate(raw, 0, raw.Length);
            return signer.GenerateSignature();
        }

        /// <summary>
        /// Open the certificate
        /// </summary>
        private  void LoadPfx(out X509Certificate[] certificates, out AsymmetricKeyEntry privateKey)
        {
            if (!string.IsNullOrEmpty(pfxFile))
            {
                // Load PFX directly
                using (var pfxStream = new FileStream(pfxFile, FileMode.Open, FileAccess.Read))
                {
                    var pfx = new Pkcs12Store(pfxStream, pfxPassword.ToCharArray());
                    var alias = pfx.Aliases.Cast<string>().FirstOrDefault(pfx.IsKeyEntry);
                    if (alias == null)
                        throw new ArgumentException("Cannot find a certificate with a key");
                    certificates = pfx.GetCertificateChain(alias).Select(x => x.Certificate).ToArray();
                    privateKey = pfx.GetKey(alias);                    
                }
            }
            else if (!string.IsNullOrEmpty(certificateThumbprint))
            {
                // Load thumbprint
                var x509 = CertificateSupport.GetCertByThumbprint(certificateThumbprint);
                if (x509 == null)
                    throw new ArgumentException("Failed to load certificate by thumbprint");
                if (!x509.HasPrivateKey)
                    throw new ArgumentException("Certificate has no private key");

                // Gets the certificates
                var parser = new X509CertificateParser();
                certificates = new[] { parser.ReadCertificate(x509.RawData) };

                // Get the private key
                var netPrivateKey = x509.PrivateKey as RSACryptoServiceProvider;
                if (netPrivateKey == null)
                    throw new ArgumentException("Private key is not an RSA crypto service provider");
                var parameters = netPrivateKey.ExportParameters(true);
                var keyParameters = new RsaPrivateCrtKeyParameters(
                    new BigInteger(1, parameters.Modulus),
                    new BigInteger(1, parameters.Exponent),
                    new BigInteger(1, parameters.D),
                    new BigInteger(1, parameters.P),
                    new BigInteger(1, parameters.Q),
                    new BigInteger(1, parameters.DP),
                    new BigInteger(1, parameters.DQ),
                    new BigInteger(1, parameters.InverseQ));
                privateKey = new AsymmetricKeyEntry(keyParameters);
            }
            else
            {
                throw new ArgumentException("No certificate specified");
            }
        }

        /// <summary>
        /// Creates a human readable fingerprint for this certificate. This fingerprint may be
        /// compared by a user with an other certificate's fingerprint to proof their equality.
        /// </summary>
        protected static string CreateFingerprint(GeneralDigest a_digestGenerator, byte[] a_data)
        {
            var digestData = new byte[a_digestGenerator.GetDigestSize()];
            a_digestGenerator.BlockUpdate(a_data, 0, a_data.Length);
            a_digestGenerator.DoFinal(digestData, 0);
            return string.Join(":", digestData.Select(x => x.ToString("X2")));
        }
    }
}
