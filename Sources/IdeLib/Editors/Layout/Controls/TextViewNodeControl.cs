using System.Windows.Controls;
using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout.Controls
{
    internal class TextViewNodeControl : TextBlock, IViewNodeControl
    {
        private readonly TextViewNode node;
        private readonly IXmlLayoutDesigner rootDesigner;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TextViewNodeControl(TextViewNode node, IXmlLayoutDesigner rootDesigner)
        {
            this.node = node;
            this.rootDesigner = rootDesigner;
            node.PropertyChanged += (s, x) => UpdateFromNode();
            UpdateFromNode();
            MouseLeftButtonUp += (s, x) => rootDesigner.Select(this);
        }

        /// <summary>
        /// Update my content from the node.
        /// </summary>
        private void UpdateFromNode()
        {
            Text = node.Text;
        }

        /// <summary>
        /// Gets access to the XML node
        /// </summary>
        public ViewNode Node { get { return node; } }
    }
}
