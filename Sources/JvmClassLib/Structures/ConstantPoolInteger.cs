namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolInteger : ConstantPoolValue<int> 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolInteger(ConstantPool constantPool, int value)
            : base(constantPool, value)
        {
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Integer; }
        }
    }
}
