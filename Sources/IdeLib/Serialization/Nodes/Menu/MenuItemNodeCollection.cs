namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// Collection of <see cref="MenuItemNode"/>
    /// </summary>
    public class MenuItemNodeCollection : NodeCollection<MenuItemNode>, IMenuChildNodeContainer
    {
        public MenuItemNodeCollection(MenuChildNode parent)
            : base("Items")
        {
            Parent = parent;
        }

        /// <summary>
        /// Is it allowed to add group nodes to this container?
        /// </summary>
        public bool CanAddGroupNodes { get { return false; } }

        /// <summary>
        /// Gets the parent of items in this container.
        /// </summary>
        public MenuChildNode Parent { get; private set; }
    }
}
