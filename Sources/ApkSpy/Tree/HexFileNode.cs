using System;
using System.Text;

namespace Dot42.ApkSpy.Tree
{
    internal class HexFileNode : TextFileNode
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public HexFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText()
        {
            var data = Load();
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                if ((i % 40) == 0)
                    sb.AppendLine();
                sb.AppendFormat("{0:X2} ", data[i]);
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }

    //[Export(typeof(INodeBuilder))]
    public class HexFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return fileName.EndsWith(".rsa", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new HexFileNode(source, fileName);
        }
    }
}
