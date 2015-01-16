namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// Collection of <see cref="MenuChildNode"/>
    /// </summary>
    public class MenuChildNodeCollection : NodeCollection<MenuChildNode>, IMenuChildNodeContainer
    {
        public MenuChildNodeCollection(MenuChildNode parent)
            : base("Menu items")
        {
            Parent = parent;
        }

        /// <summary>
        /// Is it allowed to add group nodes to this container?
        /// </summary>
        public bool CanAddGroupNodes { get { return true; } }

        /// <summary>
        /// Gets the parent of items in this container.
        /// </summary>
        public MenuChildNode Parent { get; private set; }
    }
}
