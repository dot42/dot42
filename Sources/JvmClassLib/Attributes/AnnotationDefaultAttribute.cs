namespace Dot42.JvmClassLib.Attributes
{
    public sealed class AnnotationDefaultAttribute: Attribute
    {
        private readonly ElementValue defaultValue;
        internal const string AttributeName = "AnnotationDefault";

        /// <summary>
        /// Default ctor
        /// </summary>
        public AnnotationDefaultAttribute(ElementValue defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the default value
        /// </summary>
        public ElementValue DefaultValue
        {
            get { return defaultValue; }
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
