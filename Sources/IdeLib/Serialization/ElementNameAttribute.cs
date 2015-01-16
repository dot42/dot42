using System;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Specifies the XML element name of a serialization node.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ElementNameAttribute : Attribute
    {
        private readonly string elementName;
        private readonly string elementNamespace;
        private readonly string[] parentNames;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ElementNameAttribute(string elementName)
            : this(elementName, null, null)
        {
        }

        /// <summary>
        /// Full ctor
        /// </summary>
        public ElementNameAttribute(string elementName, string elementNamespace, params string[] parentNames)
        {
            this.elementName = elementName;
            this.elementNamespace = elementNamespace;
            this.parentNames = parentNames;
        }

        /// <summary>
        /// Local name of element name
        /// </summary>
        public string ElementName
        {
            get { return elementName; }
        }

        /// <summary>
        /// Namespace of element name
        /// </summary>
        public string ElementNamespace
        {
            get { return elementNamespace; }
        }

        /// <summary>
        /// Local name of the required parent element.
        /// Set to null or empty array for any parent.
        /// </summary>
        public string[] ParentNames
        {
            get { return parentNames; }
        }

        /// <summary>
        /// If set, this element must be the root element
        /// </summary>
        public bool RootElement { get; set; }
    }
}
