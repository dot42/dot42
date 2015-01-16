using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout.Controls
{
    internal class LinearLayoutControl : StackPanel, IViewGroupNodeControl
    {
        private readonly LinearLayoutNode node;
        private readonly IXmlLayoutDesigner rootDesigner;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LinearLayoutControl(LinearLayoutNode node, IXmlLayoutDesigner rootDesigner)
        {
            this.node = node;
            this.rootDesigner = rootDesigner;
            node.PropertyChanged += (s, x) => UpdateFromNode();
            ViewGroupChildConnector.Connect(node, this, rootDesigner);
            UpdateFromNode();
        }

        /// <summary>
        /// Update my content from the node.
        /// </summary>
        private void UpdateFromNode()
        {
            Orientation = (node.Orientation == "horizontal") ? Orientation.Horizontal : Orientation.Vertical;
        }

        /// <summary>
        /// Gets access to the XML node
        /// </summary>
        public ViewNode Node { get { return node; } }

        /// <summary>
        /// Add a control representing a child view.
        /// </summary>
        void IViewGroupNodeControl.Add(IViewNodeControl childControl)
        {
            Children.Add((UIElement) childControl);
        }

        /// <summary>
        /// Remove a control representing a child view.
        /// </summary>
        void IViewGroupNodeControl.Remove(IViewNodeControl childControl)
        {
            Children.Remove((UIElement)childControl);
        }

        /// <summary>
        /// Gets all controls representing child views.
        /// </summary>
        IEnumerable<IViewNodeControl> IViewGroupNodeControl.Children
        {
            get { return Children.OfType<IViewNodeControl>(); }
        }
    }
}
