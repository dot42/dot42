namespace Dot42.JvmClassLib.Attributes
{
    public sealed class DeprecatedAttribute : Attribute
    {
        internal const string AttributeName = "Deprecated";

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }
    }
}
