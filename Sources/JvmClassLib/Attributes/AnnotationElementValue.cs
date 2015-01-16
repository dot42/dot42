namespace Dot42.JvmClassLib.Attributes
{
    /// <summary>
    /// Represent an element_value structure with annotation_value.
    /// CLASS FILE FORMAT 4.7.16.1
    /// </summary>
    public sealed class AnnotationElementValue : ElementValue
    {
        private readonly Annotation annotation;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AnnotationElementValue(Annotation annotation)
        {
            this.annotation = annotation;
        }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public override char Tag { get { return '@'; } }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(IElementValueVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the annotation
        /// </summary>
        public Annotation Annotation
        {
            get { return annotation; }
        }
    }
}