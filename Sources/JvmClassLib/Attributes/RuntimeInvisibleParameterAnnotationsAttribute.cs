namespace Dot42.JvmClassLib.Attributes
{
    public sealed class RuntimeInvisibleParameterAnnotationsAttribute: AnnotationsAttribute 
    {
        internal const string AttributeName = "RuntimeInvisibleParameterAnnotations";

        /// <summary>
        /// Default ctor
        /// </summary>
        public RuntimeInvisibleParameterAnnotationsAttribute(Annotation[] annotations)
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
