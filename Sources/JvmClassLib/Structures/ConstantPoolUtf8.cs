namespace Dot42.JvmClassLib.Structures
{
    internal sealed class ConstantPoolUtf8 : ConstantPoolValue<string>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ConstantPoolUtf8(ConstantPool constantPool, string value)
            : base(constantPool, value)
        {
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Utf8; }
        }
    }
}
