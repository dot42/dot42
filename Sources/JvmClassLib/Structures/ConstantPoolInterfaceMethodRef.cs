namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolInterfaceMethodRef : ConstantPoolMethodRef
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolInterfaceMethodRef(ConstantPool constantPool, int classIndex, int nameAndTypeIndex)
            : base(constantPool, classIndex, nameAndTypeIndex)
        {
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.InterfaceMethodref; }
        }
    }
}
