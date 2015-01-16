namespace Dot42.JvmClassLib.Attributes
{
    public sealed class RuntimeVisibleParameterAnnotationsAttribute: AnnotationsAttribute 
    {
        internal const string AttributeName = "RuntimeVisibleParameterAnnotations";

        /// <summary>
        /// Default ctor
        /// </summary>
        public RuntimeVisibleParameterAnnotationsAttribute(Annotation[] annotations)
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
