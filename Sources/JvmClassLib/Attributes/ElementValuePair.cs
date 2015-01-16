using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value_pair structure.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public sealed class ElementValuePair
    {
        private readonly ConstantPool cp;
        private readonly int elementNameIndex;
        private readonly ElementValue value;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ElementValuePair(ConstantPool cp, int elementNameIndex, ElementValue value)
        {
            this.cp = cp;
            this.elementNameIndex = elementNameIndex;
            this.value = value;
        }

        public string ElementName
        {
            get { return ((ConstantPoolUtf8)cp[elementNameIndex]).Value; }
        }

        public ElementValue Value { get { return value; } }
    }
}
