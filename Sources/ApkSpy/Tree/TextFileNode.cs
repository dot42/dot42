using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Dot42.ApkLib;

namespace Dot42.ApkSpy.Tree
{
    internal class TextFileNode : FileNode
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public TextFileNode(SourceFile source, string fileName)
            : base(source, fileName)
        {
        }

        /// <summary>
        /// Create a view to display the Types of this node.
        /// </summary>
        internal override Control CreateView(ISpyContext settings)
        {
            var tb = new TextBox();
            tb.ReadOnly = true;
            tb.MaxLength = 64 * 1024 * 1024;
            tb.Multiline = true;
            tb.ScrollBars = ScrollBars.Both;
            tb.WordWrap = false;
            try
            {
                tb.Text = LoadText();
            }
            catch (Exception ex)
            {
                tb.Text = string.Format("Error: {0}\n\n{1}", ex.Message, ex.StackTrace);
            }
            return tb;
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected virtual string LoadText()
        {
            return AndroidEncodings.UTF8.GetString(Load());
        }
    }

    [Export(typeof(INodeBuilder))]
    public class TextFileNodeBuilder : INodeBuilder
    {
        /// <summary>
        /// Can this builder create a node for the given filename in the given APK?
        /// </summary>
        public bool Supports(SourceFile source, string fileName)
        {
            fileName = fileName.ToLower();
            if (fileName.Contains("meta-inf"))
            {
                return fileName.EndsWith(".mf") || fileName.EndsWith(".sf");
            }
            return false;
        }

        /// <summary>
        /// Create a node for the given filename in the given APK?
        /// </summary>
        public Node Create(SourceFile source, string fileName)
        {
            return new TextFileNode(source, fileName);
        }
    }
}
