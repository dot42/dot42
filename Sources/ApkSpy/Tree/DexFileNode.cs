using System;
using System.ComponentModel.Composition;
using System.IO;
using Dot42.DexLib;

namespace Dot42.ApkSpy.Tree
{
    internal class DexFileNode : TextFileNode
    {
        private Dex dex;
        private int dexSize;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
            var data = Load();
            dexSize = data.Length;
            dex = Dex.Read(new MemoryStream(data));
            foreach (var classDef in dex.Classes)
            {
                var parentNodes = Nodes.GetParentForFile(classDef.Namespace, 7, new[] { '.' });
                parentNodes.Add(new DexClassDefinitionNode(classDef));
            }
        }

        protected override string LoadText()
        {
            EnsureChildNodesCreated();
            return string.Format("Size {0}kb,  ({1} bytes)", (dexSize / 1024), dexSize);
        }
    }

    [Export(typeof(INodeBuilder))]
    public class DexFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            return fileName.EndsWith(".dex", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new DexFileNode(source, fileName);
        }
    }
}
