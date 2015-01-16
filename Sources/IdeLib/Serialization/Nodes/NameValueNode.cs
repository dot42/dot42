using System;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.ResourcesLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes
{
    /// <summary>
    /// Base class for various simple nodes
    /// </summary>
    [Obfuscation(Feature = "@SerializableNode")]
    public abstract class NameValueNode : ValueNode
    {
        private readonly PropertyInfo nameProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected NameValueNode()
        {
            nameProperty = ReflectionHelper.PropertyOf(() => Name);
        }

        [AttributeName("name")]
        [Obfuscation(Feature = "@Xaml")]
        public string Name
        {
            get { return Get(nameProperty); }
            set
            {
                string errorMessage;
                if (!ResourceValidators.ValidateIdNameValue(ref value, out errorMessage))
                {
                    if (IsEditing)
                        throw new ArgumentException(errorMessage);
                }
                Set(nameProperty, value);
            }
        }
    }
}
