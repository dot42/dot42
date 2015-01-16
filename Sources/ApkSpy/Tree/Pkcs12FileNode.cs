using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Pkcs;

namespace Dot42.ApkSpy.Tree
{
    internal class Pkcs12FileNode : TextFileNode
    {
        private string password;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Pkcs12FileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText()
        {
            if (password == null)
            {
                password = PasswordDialog.Ask();
            }
            if (password == null) return string.Empty;
            var store = new Pkcs12Store(new MemoryStream(Load()), password.ToCharArray());
            return string.Join(", " + Environment.NewLine, store.Aliases.Cast<string>());
        }
    }

    [Export(typeof(INodeBuilder))]
    public class Pkcs12FileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return fileName.EndsWith(".p12", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new Pkcs12FileNode(source, fileName);
        }
    }
}
