using System;
using System.Reflection;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/integer
    /// </summary>
    [ElementName("integer")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class IntegerNode : NameValueNode
    {
        /// <summary>
        /// Validate a new value for <see cref="NameValueNode.Value"/>.
        /// </summary>
        protected override string ValidateValue(string value)
        {
            value = (value ?? string.Empty);
            int x;
            if (!int.TryParse(value, out x))
                throw new ArgumentException("Integer expected");
            return value;
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
