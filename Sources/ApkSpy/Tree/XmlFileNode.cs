using System;
using System.ComponentModel.Composition;
using System.IO;
using Dot42.ApkLib.Resources;

namespace Dot42.ApkSpy.Tree
{
    internal class XmlFileNode : TextFileNode
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText()
        {
            var raw = Load();
            var xmlTree = new XmlTree(new MemoryStream(raw));
            return xmlTree.AsXml().ToString();
        }
    }

    [Export(typeof(INodeBuilder))]
    public class XmlFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return source.IsApk && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new XmlFileNode(source, fileName);
        }
    }
}
