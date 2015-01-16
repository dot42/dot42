using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout.Controls
{
    internal class ViewNodeControl<TNode, TControl> : ContentControl, IViewNodeControl
        where TNode: ViewNode
        where TControl:  UIElement, new()
    {
        private readonly TNode node;
        private readonly TControl control;
        private readonly IXmlLayoutDesigner rootDesigner;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ViewNodeControl(TNode node, IXmlLayoutDesigner rootDesigner)
        {
            this.node = node;
            this.rootDesigner = rootDesigner;
            control = new TControl();
            node.PropertyChanged += (s, x) => UpdateFromNode();
            UpdateFromNode();
            MouseLeftButtonUp += (s, x) => rootDesigner.Select(this);
        }

        /// <summary>
        /// Update my content from the node.
        /// </summary>
        private void UpdateFromNode()
        {
            Content = null;
            if (node.IsSelected)
            {
                Content = new Border { Child = control, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1) };
            }
            else
            {
                Content = control;
            }
            UpdateFromNode(control, node);
        }

        /// <summary>
        /// Update my content from the node.
        /// </summary>
        protected virtual void UpdateFromNode(TControl control, TNode node)
        {
        }

        /// <summary>
        /// Gets access to the XML node
        /// </summary>
        public TNode Node { get { return node; } }

        /// <summary>
        /// Gets access to the XML node
        /// </summary>
        ViewNode IViewNodeControl.Node { get { return node; } }
    }
}
