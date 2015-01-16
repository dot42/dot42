namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value structure with array_value.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public sealed class ArrayElementValue : ElementValue
    {
        private readonly ElementValue[] array;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ArrayElementValue(ElementValue[] array)
        {
            this.array = array;
        }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public override char Tag { get { return '['; } }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(IElementValueVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the array
        /// </summary>
        public ElementValue[] Array
        {
            get { return array; }
        }
    }
}