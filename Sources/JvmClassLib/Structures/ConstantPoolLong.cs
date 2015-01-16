namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolLong : ConstantPoolValue<long> 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolLong(ConstantPool constantPool, long value)
            : base(constantPool, value)
        {
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Long; }
        }
    }
}
