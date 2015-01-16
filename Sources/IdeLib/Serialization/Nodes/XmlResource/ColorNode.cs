using System;
using System.Reflection;
using Dot42.ResourcesLib;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/color
    /// </summary>
    [ElementName("color")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class ColorNode : NameValueNode
    {
        protected override string ValidateValue(string value)
        {
            string errorMessage;
            if (!ResourceValidators.ValidateColorValue(ref value, out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }
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
