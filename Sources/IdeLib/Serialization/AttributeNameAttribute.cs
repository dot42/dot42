using System;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Specifies the XML attribute name of a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AttributeNameAttribute : Attribute
    {
        private readonly string attributeName;
        private readonly string attributeNamespace;

        public AttributeNameAttribute(string attributeName)
            : this(attributeName, null)
        {
        }

        public AttributeNameAttribute(string attributeName, string attributeNamespace)
        {
            this.attributeName = attributeName;
            this.attributeNamespace = attributeNamespace;
        }

        public string AttributeName
        {
            get { return attributeName; }
        }

        public string AttributeNamespace
        {
            get { return attributeNamespace; }
        }
    }
}
