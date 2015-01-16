using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value structure with const_value_index.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public sealed class ConstElementValue : ElementValue
    {
        private readonly ConstantPool cp;
        private readonly char tag;
        private readonly int valueIndex;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstElementValue(ConstantPool cp, char tag, int valueIndex)
        {
            this.cp = cp;
            this.tag = tag;
            this.valueIndex = valueIndex;
        }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public override char Tag { get { return tag; } }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(IElementValueVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the constant value
        /// </summary>
        public object Value
        {
            get { return ((IConstantPoolValue)cp[valueIndex]).Value; }
        }

        public override string ToString()
        {
            return string.Format("Value:{0}", Value);
        }
    }
}