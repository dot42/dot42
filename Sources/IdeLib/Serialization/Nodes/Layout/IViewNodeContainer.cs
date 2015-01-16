namespace Dot42.Ide.Serialization.Nodes.Layout
{
    public interface IViewNodeContainer : ISerializationNodeList
    {
        /// <summary>
        /// Gets the parent of items in this container.
        /// </summary>
        ViewGroupNode Parent { get; }
    }
}
