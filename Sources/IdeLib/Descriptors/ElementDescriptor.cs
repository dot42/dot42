using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// XML element descriptor, used to provide information about possible child elements and attributes.
    /// </summary>
    [DebuggerDisplay("{@Name}")]
    public class ElementDescriptor : IElementDescriptorProvider
    {
        private readonly string name;
        private readonly Dictionary<string, AttributeDescriptor> attributes = new Dictionary<string, AttributeDescriptor>();
        private readonly Dictionary<string, ElementDescriptor> children = new Dictionary<string, ElementDescriptor>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public ElementDescriptor(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the element name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Add the given attribute as possible attribute for this element if no other attribute with same name exists.
        /// </summary>
        internal bool Add(AttributeDescriptor attribute)
        {
            if (!attributes.ContainsKey(attribute.Name))
            {
                attributes[attribute.Name] = attribute;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add all given attributes.
        /// </summary>
        internal void AddRange(IEnumerable<AttributeDescriptor> attributes)
        {
            foreach (var a in attributes)
            {
                Add(a);
            }
        }

        /// <summary>
        /// Add the given element as possible child element for this element.
        /// </summary>
        internal void Add(ElementDescriptor child)
        {
            children[child.Name] = child;
        }

        /// <summary>
        /// Add all given elements.
        /// </summary>
        internal void AddRange(IEnumerable<ElementDescriptor> children)
        {
            foreach (var c in children)
            {
                Add(c);
            }
        }

        /// <summary>
        /// Gets all possible attributes
        /// </summary>
        public IEnumerable<AttributeDescriptor> Attributes
        {
            get { return attributes.Values; }
        }

        /// <summary>
        /// Gets all possible child elements.
        /// </summary>
        internal IEnumerable<ElementDescriptor> Children
        {
            get { return children.Values; }
        }

        /// <summary>
        /// Gets all provided element descriptors.
        /// </summary>
        IEnumerable<ElementDescriptor> IElementDescriptorProvider.Descriptors
        {
            get { return Children; }
        }
    }
}
