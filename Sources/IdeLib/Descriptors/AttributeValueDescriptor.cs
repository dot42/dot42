namespace Dot42.Ide.Descriptors
{
    /// <summary>
    /// Descriptor of a possible value of an XML attribute.
    /// </summary>
    public class AttributeValueDescriptor
    {
        private readonly string value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AttributeValueDescriptor(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the value (name)
        /// </summary>
        public string Value
        {
            get { return value; }
        }
    }
}
