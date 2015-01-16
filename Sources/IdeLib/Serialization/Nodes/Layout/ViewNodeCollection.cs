namespace Dot42.Ide.Serialization.Nodes.Layout
{
    /// <summary>
    /// Collection of <see cref="ViewNode"/>
    /// </summary>
    public class ViewNodeCollection : NodeCollection<ViewNode>, IViewNodeContainer 
    {
        public ViewNodeCollection(ViewGroupNode parent)
            : base("Children")
        {
            Parent = parent;
        }

        /// <summary>
        /// Gets the parent of items in this container.
        /// </summary>
        public ViewGroupNode Parent { get; private set; }
    }
}
