namespace Dot42.JvmClassLib.Attributes
{
    public sealed class OverrideAttribute : Attribute
    {
        internal const string AttributeName = "Override";

        /// <summary>
        /// Gets the name of this attribute
        /// </summary>
        public override string Name
        {
            get { return AttributeName; }
        }
    }
}
