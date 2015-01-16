using System.Reflection;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes
{
    /// <summary>
    /// Base class for various simple nodes
    /// </summary>
    [Obfuscation(Feature = "@SerializableNode")]
    public abstract class ValueNode : SerializationNode
    {
        private readonly PropertyInfo valueProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected ValueNode()
        {
            valueProperty = ReflectionHelper.PropertyOf(() => Value);
        }

        /// <summary>
        /// Value of the node.
        /// </summary>
        [Value]
        [Obfuscation(Feature = "@Xaml")]
        public virtual string Value
        {
            get { return Get(valueProperty); }
            set
            {
                if (IsEditing) value = ValidateValue(value);
                Set(valueProperty, value);
            }
        }

        /// <summary>
        /// Validate a new value for <see cref="Value"/>.
        /// </summary>
        protected virtual string ValidateValue(string value)
        {
            return value;
        }
    }
}
