namespace Dot42.JvmClassLib.Attributes
{
    public sealed class RuntimeVisibleAnnotationsAttribute : AnnotationsAttribute 
    {
        internal const string AttributeName = "RuntimeVisibleAnnotations";

        /// <summary>
        /// Default ctor
        /// </summary>
        public RuntimeVisibleAnnotationsAttribute(Annotation[] annotations)
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
