using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/integer-array/item
    /// </summary>
    [ElementName("item", null, "integer-array")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class IntegerArrayItemNode : ValueNode
    {
        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}
