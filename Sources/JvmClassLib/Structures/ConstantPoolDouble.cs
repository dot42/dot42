namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolDouble : ConstantPoolValue<double> 
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolDouble(ConstantPool constantPool, double value)
            : base(constantPool, value)
        {
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Double; }
        }
    }
}
