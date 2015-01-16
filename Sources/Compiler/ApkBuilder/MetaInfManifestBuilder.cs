using System.IO;

namespace Dot42.ApkBuilder
{
    /// <summary>
    /// Builder of the META-INF/MANIFEST.MF file
    /// </summary>
    internal sealed class MetaInfManifestBuilder : MetaInfBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public MetaInfManifestBuilder()
            : this(false)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        private MetaInfManifestBuilder(bool empty)
        {
            if (!empty)
            {
                AppendLine("Manifest-Version: 1.0");
                AppendLine("Created-By: 1.6.0_32-ea (Sun Microsystems Inc.)");
                AppendLine();
            }
        }

        /// <summary>
        /// Gets the raw data of the main attributes
        /// </summary>
        public byte[] MainAttributes
        {
            get { return new MetaInfManifestBuilder().ToArray(); }
        }


        /// <summary>
        /// Add a Digest entry
        /// </summary>
        public byte[] GetEntryAttributes(string name, Stream stream)
        {
            var builder = new MetaInfManifestBuilder(true);
            builder.AddSha1Digest(name, stream);
            return builder.ToArray();
        }

    }
}
