namespace Dot42.JvmClassLib.Attributes
{
    public sealed class SignatureAttribute : Attribute
    {
        internal const string AttributeName = "Signature";

        private readonly string value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SignatureAttribute(string value)
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
