namespace Dot42.Ide.Serialization.Nodes.Menu
{
    public interface IMenuChildNodeContainer : ISerializationNodeList
    {
        /// <summary>
        /// Is it allowed to add group nodes to this container?
        /// </summary>
        bool CanAddGroupNodes { get; }

        /// <summary>
        /// Gets the parent of items in this container.
        /// </summary>
        MenuChildNode Parent { get; }
    }
}
