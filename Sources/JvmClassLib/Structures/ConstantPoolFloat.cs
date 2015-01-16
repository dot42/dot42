namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolFloat : ConstantPoolValue<float> 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolFloat(ConstantPool constantPool, float value)
            : base(constantPool, value)
        {
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Float; }
        }
    }
}
