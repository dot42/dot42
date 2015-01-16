using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// XML attribute descriptor, used to provide possible values for the attribute.
    /// </summary>
    [DebuggerDisplay("{@Name}")]
    public class AttributeDescriptor
    {
        private readonly string name;
        private readonly AttributeFormat format;
        private readonly Dictionary<string,AttributeValueDescriptor> values = new Dictionary<string, AttributeValueDescriptor>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public AttributeDescriptor(string name, AttributeFormat format)
        {
            this.name = name;
            this.format = format;
            if ((format & AttributeFormat.Boolean) == AttributeFormat.Boolean)
            {
                Add(new AttributeValueDescriptor("true"));
                Add(new AttributeValueDescriptor("false"));
            }
        }

        /// <summary>
        /// Add a possible value.
        /// </summary>
        public void Add(AttributeValueDescriptor descriptor)
        {
            if (!values.ContainsKey(descriptor.Value))
                values[descriptor.Value] = descriptor;
        }

        /// <summary>
        /// Formats allowed for this attribute
        /// </summary>
        public AttributeFormat Format
        {
            get { return format; }
        }

        /// <summary>
        /// Gets attribute name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Does this attribute have a fixed set of attribute values?
        /// </summary>
        public bool HasStandardValues
        {
            get { return (values != null) && (values.Count > 0); }
        }

        /// <summary>
        /// Gets all possible (standard) values.
        /// </summary>
        public IEnumerable<AttributeValueDescriptor> StandardValues
        {
            get { return values.Values; }
        }
    }
}
