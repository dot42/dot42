namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value structure.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public abstract class ElementValue
    {
        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public abstract char Tag { get; }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public abstract TReturn Accept<TReturn, TData>(IElementValueVisitor<TReturn, TData> visitor, TData data);
    }
}
