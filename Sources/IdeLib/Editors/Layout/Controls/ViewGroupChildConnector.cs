using System.Collections.Specialized;
using System.Linq;
using Dot42.Ide.Serialization.Nodes.Layout;

namespace Dot42.Ide.Editors.Layout.Controls
{
    /// <summary>
    /// Helper to connect changes in ViewGroupNode to IViewGroupNodeControl.
    /// </summary>
    internal class ViewGroupChildConnector
    {
        private readonly ViewGroupNode node;
        private readonly IViewGroupNodeControl control;
        private readonly IXmlLayoutDesigner rootDesigner;

        public static void Connect(ViewGroupNode node, IViewGroupNodeControl control, IXmlLayoutDesigner rootDesigner)
        {
            var connector = new ViewGroupChildConnector(node, control, rootDesigner);
            node.Children.CollectionChanged += connector.OnChildrenChanged;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        private ViewGroupChildConnector(ViewGroupNode node, IViewGroupNodeControl control, IXmlLayoutDesigner rootDesigner)
        {
            this.node = node;
            this.control = control;
            this.rootDesigner = rootDesigner;
            // Add controls for all children now
            foreach (var childNode in node.Children)
            {
                var childControl = childNode.Accept(ControlBuilder.Instance, rootDesigner);
                control.Add(childControl);                
            }
        }

        /// <summary>
        /// Children added/removed
        /// </summary>
        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var childNode in e.NewItems.Cast<ViewNode>())
                {
                    var childControl = childNode.Accept(ControlBuilder.Instance, rootDesigner);
                    control.Add(childControl);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var childNode in e.OldItems.Cast<ViewNode>())
                {
                    var childControl = control.Children.FirstOrDefault(x => x.Node == childNode);
                    if (childControl != null)
                    {
                        control.Remove(childControl);
                    }
                }
            }
        }
    }
}
