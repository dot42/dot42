using System.IO;

namespace Dot42.ApkBuilder
{
    /// <summary>
    /// Builder of the META-INF/CERT.SF file
    /// </summary>
    internal class MetaInfCertSfBuilder : MetaInfBuilder
    {
        private const string ManifestDigestMarker = "##$$@@";
        private const string ManifestMainAttributesDigestMarker = "@@$$##";
        private readonly MetaInfManifestBuilder manifest;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MetaInfCertSfBuilder(MetaInfManifestBuilder manifest)
        {
            this.manifest = manifest;
            AppendLine("Signature-Version: 1.0");
            AppendLine("SHA1-Digest-Manifest-Main-Attributes: " + ManifestMainAttributesDigestMarker);
            AppendLine("Created-By: 1.6.0_32-ea (Sun Microsystems Inc.)");
            AppendLine("SHA1-Digest-Manifest: " + ManifestDigestMarker);
            AppendLine();
        }

        /// <summary>
        /// Add a Digest entry
        /// </summary>
        public new void AddSha1Digest(string name, Stream stream)
        {
            var raw = manifest.GetEntryAttributes(name, stream);
            AppendLine(string.Format("Name: {0}", name.Replace('\\', '/')));
            AppendLine(string.Format("SHA1-Digest: {0}", CreateSha1Digest(new MemoryStream(raw))));
            AppendLine();
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();
            var digest = CreateSha1Digest(new MemoryStream(manifest.ToArray()));
            result = result.Replace(ManifestDigestMarker, digest);
            digest = CreateSha1Digest(new MemoryStream(manifest.MainAttributes));
            result = result.Replace(ManifestMainAttributesDigestMarker, digest);
            return result;
        }
    }
}
