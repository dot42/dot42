namespace Dot42.JvmClassLib.Attributes
{
    public sealed class ConstantValueAttribute : Attribute
    {
        internal const string AttributeName = "ConstantValue";

        private readonly object value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConstantValueAttribute(object value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the constant value
        /// </summary>
        public object Value
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
