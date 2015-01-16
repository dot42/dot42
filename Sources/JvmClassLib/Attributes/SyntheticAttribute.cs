namespace Dot42.JvmClassLib.Attributes
{
    public sealed class SyntheticAttribute : Attribute
    {
        internal const string AttributeName = "Synthetic";

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }
    }
}
