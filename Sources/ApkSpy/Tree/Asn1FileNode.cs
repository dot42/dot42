using System;
using System.ComponentModel.Composition;
using System.IO;
using Dot42.ApkLib;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Utilities;

namespace Dot42.ApkSpy.Tree
{
    internal class Asn1FileNode : TextFileNode
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public Asn1FileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText()
        {
            var asn1 = new Asn1InputStream(new MemoryStream(Load())).ReadObject();
            return Asn1Dump.DumpAsString(asn1);
        }
    }

    [Export(typeof(INodeBuilder))]
    public class CertRsaFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return fileName.EndsWith(".rsa", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".ec", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new Asn1FileNode(source, fileName);
        }
    }
}
