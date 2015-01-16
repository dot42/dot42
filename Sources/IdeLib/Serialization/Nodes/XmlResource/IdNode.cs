using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/item
    /// </summary>
    [ElementName("item", null, "resources")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class IdNode : NameNode
    {
        [AttributeName("type")]
        [Obfuscation(Feature = "@Xaml")]
        public string Type
        {
            get { return "id"; }
            set { /* ignore */}
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}
