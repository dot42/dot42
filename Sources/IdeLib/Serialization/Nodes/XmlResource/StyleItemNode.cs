using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/style/item
    /// </summary>
    [ElementName("item", null, "style")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class StyleItemNode : NameValueNode
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
