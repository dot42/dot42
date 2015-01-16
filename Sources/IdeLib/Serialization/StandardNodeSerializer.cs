using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Class used to serialize/deserialize XML nodes.
    /// </summary>
    public class StandardNodeSerializer<T> : NodeSerializer
        where T : SerializationNode, new()
    {
        private readonly XName elementName;
        private readonly string[] parentNames;
        private readonly List<PropertySerializer> properties;
        private readonly bool rootElement;
        private readonly List<AddNamespaceAttribute> addNamespaces;

        /// <summary>
        /// Default ctor
        /// </summary>
        public StandardNodeSerializer()
        {
            var type = typeof (T);
            var enAttr = GetAttribute<ElementNameAttribute>(type, true);
            elementName = XName.Get(enAttr.ElementName, enAttr.ElementNamespace ?? string.Empty);
            parentNames = enAttr.ParentNames;
            rootElement = enAttr.RootElement;

            addNamespaces = GetAttributes<AddNamespaceAttribute>(type).ToList();

            var allProperties = TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>().ToList();
            properties = new List<PropertySerializer>();
            properties.AddRange(allProperties.Where(HasAttribute<AttributeNameAttribute>).Select(x => new AttributePropertySerializer(x)));
            properties.AddRange(allProperties.Where(HasAttribute<ValueAttribute>).Select(x => new ElementValuePropertySerializer(x)));
        }

        /// <summary>
        /// Does the given property have an attribute of type U?
        /// </summary>
        private static bool HasAttribute<U>(PropertyDescriptor descriptor)
        {
            return descriptor.Attributes.OfType<U>().Any();
        }

        /// <summary>
        /// Can the given element be de-serialized?
        /// </summary>
        public override bool CanDeserialize(XElement element)
        {
            var name = element.Name;
            if (name != elementName)
                return false;
            if ((parentNames != null) && (parentNames.Length > 0))
            {
                var parent = element.Parent;
                var actualParentName = (parent != null) ? parent.Name.LocalName : null;
                if (!parentNames.Contains(actualParentName))
                    return false;
            }
            if (rootElement)
            {
                if (element.Parent != null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// De-serialize the given element into a node.
        /// </summary>
        public override SerializationNode Deserialize(XElement element, ISerializationNodeContainer parent)
        {
            var node = new T();
            properties.ForEach(x => x.Load(element, node));
            return node;
        }

        /// <summary>
        /// Can the given node be serialized?
        /// </summary>
        public override bool CanSerialize(SerializationNode node)
        {
            return (node.GetType() == typeof (T));
        }

        /// <summary>
        /// Serialize the given node into an element.
        /// </summary>
        public override XElement Serialize(SerializationNode node, ISerializationNodeContainer parent)
        {
            var element = new XElement(elementName);
            if (parent == null)
            {
                foreach (var attr in addNamespaces)
                {
                    element.Add(new XAttribute(XNamespace.Xmlns + attr.Prefix, attr.Namespace));
                }
            }
            properties.ForEach(x => x.Store(element, node));
            return element;
        }

        /// <summary>
        /// Gets an attribute from the given provider.
        /// </summary>
        private static TAttr GetAttribute<TAttr>(ICustomAttributeProvider provider, bool failOnMissing)
            where TAttr : Attribute
        {
            var attrs = provider.GetCustomAttributes(typeof (TAttr), false);
            if (attrs.Length != 1)
            {
                if (failOnMissing)
                {
                    throw new ArgumentException(string.Format("Missing attribute {0} on {1}", typeof (TAttr).Name,
                                                              provider));
                }
                return null;
            }
            return (TAttr) attrs[0];
        }

        /// <summary>
        /// Gets all attributes from the given provider.
        /// </summary>
        private static IEnumerable<TAttr> GetAttributes<TAttr>(ICustomAttributeProvider provider)
            where TAttr : Attribute
        {
            var attrs = provider.GetCustomAttributes(typeof(TAttr), false);
            return attrs.Cast<TAttr>();
        }

        private abstract class PropertySerializer
        {
            private readonly PropertyDescriptor property;
            private readonly TypeConverter typeConverter;
            private readonly object defaultValue;

            /// <summary>
            /// Default ctor
            /// </summary>
            protected PropertySerializer(PropertyDescriptor property)
            {
                this.property = property;
                typeConverter = property.Converter;
                var defaultValueAttr = property.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                defaultValue = (defaultValueAttr != null) ? defaultValueAttr.Value : null;
            }

            /// <summary>
            /// Load the value for this property from the XML.
            /// </summary>
            public void Load(XElement element, SerializationNode node)
            {
                string value;
                if (TryGetValue(element, out value))
                {
                    var objectValue = typeConverter.ConvertFromString(value);
                    property.SetValue(node, objectValue);
                }
                else if (defaultValue != null)
                {
                    // Initialize to default value 
                    property.SetValue(node, defaultValue);
                }
            }

            /// <summary>
            /// Stor the value for this property into the XML.
            /// </summary>
            public void Store(XElement element, SerializationNode node)
            {
                var objectValue = property.GetValue(node);
                if ((!IsNull(objectValue)) && ((defaultValue == null) || !Equals(defaultValue, objectValue)))
                {
                    var value = typeConverter.ConvertToString(objectValue);
                    SetValue(element, value);
                }
            }

            /// <summary>
            /// Is the given value considered null?
            /// </summary>
            private static bool IsNull(object value)
            {
                return string.IsNullOrEmpty(value as string);
            }

            /// <summary>
            /// Gets the value for this property from the given XML.
            /// </summary>
            protected abstract bool TryGetValue(XElement element, out string value);

            /// <summary>
            /// Sets the value for this property from the given XML.
            /// </summary>
            protected abstract void SetValue(XElement element, string value);
        }

        private class AttributePropertySerializer : PropertySerializer
        {
            private readonly XName attributeName;

            public AttributePropertySerializer(PropertyDescriptor property)
                : base(property)
            {
                var attrName = (AttributeNameAttribute)property.Attributes[typeof (AttributeNameAttribute)];
                attributeName = XName.Get(attrName.AttributeName, attrName.AttributeNamespace ?? string.Empty);
            }

            /// <summary>
            /// Gets the value for this property from the given XML.
            /// </summary>
            protected override bool TryGetValue(XElement element, out string value)
            {
                var attr = element.Attribute(attributeName);
                value = null;
                if (attr == null)
                    return false;
                value = attr.Value;
                return true;
            }

            /// <summary>
            /// Sets the value for this property from the given XML.
            /// </summary>
            protected override void SetValue(XElement element, string value)
            {
                element.SetAttributeValue(attributeName, value);
            }
        }

        private class ElementValuePropertySerializer : PropertySerializer
        {
            public ElementValuePropertySerializer(PropertyDescriptor property)
                : base(property)
            {
            }

            /// <summary>
            /// Gets the value for this property from the given XML.
            /// </summary>
            protected override bool TryGetValue(XElement element, out string value)
            {
                value = element.Value;
                return true;
            }

            /// <summary>
            /// Sets the value for this property from the given XML.
            /// </summary>
            protected override void SetValue(XElement element, string value)
            {
                element.SetValue(value);
            }
        }
    }
}
