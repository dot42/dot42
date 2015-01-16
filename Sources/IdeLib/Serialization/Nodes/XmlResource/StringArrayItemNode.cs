using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/string-array/item
    /// </summary>
    [ElementName("item", null, "string-array")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class StringArrayItemNode : ValueNode
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
