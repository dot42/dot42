using System;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.ResourcesLib;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/bool
    /// </summary>
    [ElementName("bool")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class BoolNode : NameValueNode
    {
        /// <summary>
        /// Validate a new value for <see cref="NameValueNode.Value"/>.
        /// </summary>
        protected override string ValidateValue(string value)
        {
            string errorMessage;
            if (!ResourceValidators.ValidateBoolValue(ref value, out errorMessage))
                throw new ArgumentException(errorMessage);
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
