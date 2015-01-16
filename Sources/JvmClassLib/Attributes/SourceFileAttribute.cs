namespace Dot42.JvmClassLib.Attributes
{
    public sealed class SourceFileAttribute : Attribute
    {
        internal const string AttributeName = "SourceFile";

        private readonly string value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SourceFileAttribute(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the source file
        /// </summary>
        public string Value
        {
            get { return value; }
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
