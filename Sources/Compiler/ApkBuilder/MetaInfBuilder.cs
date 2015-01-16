using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Dot42.ApkBuilder
{
    /// <summary>
    /// Base class for builders of the META-INF/... files
    /// </summary>
    internal abstract class MetaInfBuilder
    {
        private readonly StringBuilder sb = new StringBuilder();

        /// <summary>
        /// Add a Digest entry
        /// </summary>
        public void AddSha1Digest(string name, Stream stream)
        {
            sb.AppendFormat("Name: {0}", name.Replace('\\', '/'));
            sb.AppendLine();
            sb.AppendFormat("SHA1-Digest: {0}", CreateSha1Digest(stream));
            sb.AppendLine();
            sb.AppendLine();
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            return sb.ToString();
        }

        /// <summary>
        /// Convert the data to a byte array used for storage.
        /// </summary>
        public byte[] ToArray()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        /// <summary>
        /// Write myself to the given stream
        /// </summary>
        public void WriteTo(Stream stream)
        {
            var data = ToArray();
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Add a line of text.
        /// </summary>
        protected void AppendLine(string line)
        {
            sb.AppendLine(line);
        }

        /// <summary>
        /// Add an empty line of text.
        /// </summary>
        protected void AppendLine()
        {
            sb.AppendLine();
        }

        /// <summary>
        /// Create a SHA1-Digest for the given stream
        /// </summary>
        protected static string CreateSha1Digest(Stream stream)
        {
            var hash = new SHA1Managed();
            var rawHash = hash.ComputeHash(stream);
            return Convert.ToBase64String(rawHash);
        }

        /// <summary>
        /// Create a SHA512-Digest for the given stream
        /// </summary>
        protected static string CreateSha512Digest(Stream stream)
        {
            var hash = new SHA512Managed();
            var rawHash = hash.ComputeHash(stream);
            return Convert.ToBase64String(rawHash);
        }
    }
}
