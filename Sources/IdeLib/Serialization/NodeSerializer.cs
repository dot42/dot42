using System.Xml.Linq;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Class used to serialize/deserialize XML nodes.
    /// </summary>
    public abstract class NodeSerializer
    {
        /// <summary>
        /// Can the given element be de-serialized?
        /// </summary>
        public abstract bool CanDeserialize(XElement element);

        /// <summary>
        /// De-serialize the given element into a node.
        /// </summary>
        public abstract SerializationNode Deserialize(XElement element, ISerializationNodeContainer parent);

        /// <summary>
        /// Can the given node be serialized?
        /// </summary>
        public abstract bool CanSerialize(SerializationNode node);

        /// <summary>
        /// Serialize the given node into an element.
        /// </summary>
        public abstract XElement Serialize(SerializationNode node, ISerializationNodeContainer parent);
    }
}
