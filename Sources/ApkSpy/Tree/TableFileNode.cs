using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.ApkLib.Resources;

namespace Dot42.ApkSpy.Tree
{
    internal class TableFileNode: TextFileNode
    {
        private Table table;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TableFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        private void LoadTable()
        {
            if (table != null)
                return;
            var raw = Load();
            table = new Table(new MemoryStream(raw));
        }

        /// <summary>
        /// Create children
        /// </summary>
        protected override void CreateChildNodes()
        {
            LoadTable();
            var pkg = table.Packages.First();
            Nodes.AddRange(pkg.TypeSpecs.Select(x => new TableTypeSpecNode(x)).ToArray());
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText()
        {
            var sb = new StringBuilder();
            LoadTable();
            foreach (var pkg in table.Packages)
            {
                sb.AppendLine(string.Format("Package: {0} / {1}", pkg.Name, pkg.Id));
            }
            return sb.ToString();
        }
    }

    [Export(typeof(INodeBuilder))]
    public class TableFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return source.IsApk && fileName.Equals("resources.arsc", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new TableFileNode(source, fileName);
        }
    }
}
