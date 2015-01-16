using System.Collections.Generic;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Container of serialization nodes.
    /// </summary>
    public interface ISerializationNodeContainer
    {
        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode Add(SerializationNode child);

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void Remove(SerializationNode child);

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> Children { get; }
    }
}
