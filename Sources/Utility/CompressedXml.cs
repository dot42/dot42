using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Dot42.Utility
{
    /// <summary>
    /// Helpers for loading and storing compressed XML streams.
    /// </summary>
    public static class CompressedXml
    {
        /// <summary>
        /// Load a compressed XML document from the given stream.
        /// </summary>
        public static XDocument Load(Stream stream)
        {
            return XDocument.Load(Decompress(stream));
        }

        /// <summary>
        /// Save the given document to the given stream.
        /// </summary>
        public static void WriteTo(XDocument document, Stream stream, Encoding encoding)
        {
            // Write to memory
            var memStream = new MemoryStream();
            using (var writer = XmlWriter.Create(memStream, new XmlWriterSettings { Encoding = encoding }))
            {
                document.WriteTo(writer);
            }
            memStream.Position = 0;

            // Compress
            var compressed = Compress(memStream);

            // Write to target stream
            compressed.CopyTo(stream);
        }

        /// <summary>
        /// Decompress (if needed) the given stream.
        /// </summary>
        private static Stream Decompress(Stream stream)
        {
            if (stream.Length > 2)
            {
                var id1 = stream.ReadByte();
                var id2 = stream.ReadByte();
                stream.Position -= 2;

                if ((id1 == 0x1F) && (id2 == 0x8B))
                {
                    // GZIP compressed
                    var memStream = new MemoryStream();
                    using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        var buffer = new byte[64 * 1024];
                        int len;
                        while ((len = gzipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memStream.Write(buffer, 0, len);
                        }
                    }
                    memStream.Position = 0;
                    return memStream;
                }
            }

            return stream;
        }

        /// <summary>
        /// Compress the given stream.
        /// </summary>
        private static Stream Compress(Stream stream)
        {
            // GZIP compressed
            var memStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress, true))
            {
                var buffer = new byte[64 * 1024];
                int len;
                while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    gzipStream.Write(buffer, 0, len);
                }
            }
            memStream.Position = 0;
            return memStream;
        }

    }
}
