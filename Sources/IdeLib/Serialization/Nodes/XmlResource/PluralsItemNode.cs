using System;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.ResourcesLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources/plurals/item
    /// </summary>
    [ElementName("item", null, "plurals")]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class PluralsItemNode : ValueNode
    {
        private readonly PropertyInfo quantityProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        public PluralsItemNode()
        {
            quantityProperty = ReflectionHelper.PropertyOf(() => Quantity);
        }

        [AttributeName("quantity")]
        [Obfuscation(Feature = "@Xaml")]
        public string Quantity
        {
            get { return Get(quantityProperty); }
            set
            {
                if (IsEditing)
                {
                    string errorMessage;
                    if (!ResourceValidators.ValidatePluralsQuantityValue(ref value, out errorMessage))
                        throw new ArgumentException(errorMessage);
                }
                Set(quantityProperty, value);
            }
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
