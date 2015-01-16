namespace Dot42.JvmClassLib.Attributes
{
    public sealed class RuntimeInvisibleAnnotationsAttribute: AnnotationsAttribute 
    {
        internal const string AttributeName = "RuntimeInvisibleAnnotations";

        /// <summary>
        /// Default ctor
        /// </summary>
        public RuntimeInvisibleAnnotationsAttribute(Annotation[] annotations)
            : base(annotations)
        {
        }

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }
    }
}
