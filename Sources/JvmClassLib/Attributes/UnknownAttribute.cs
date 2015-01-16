namespace Dot42.JvmClassLib.Attributes
{
    public sealed class UnknownAttribute : Attribute
    {
        private readonly string name;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal UnknownAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name { get { return name; } }
    }
}
