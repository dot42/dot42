namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Container of serialization nodes.
    /// </summary>
    public interface ISerializationNodeList : ISerializationNodeContainer
    {
        /// <summary>
        /// Gets the number of children in this container
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a node at the given index.
        /// </summary>
        SerializationNode this[int index] { get; }

        /// <summary>
        /// Gets the index of the given child in this container.
        /// </summary>
        int IndexOf(SerializationNode child);

        /// <summary>
        /// Move the given child to a new index.
        /// </summary>
        void MoveTo(SerializationNode child, int newIndex);
    }
}
