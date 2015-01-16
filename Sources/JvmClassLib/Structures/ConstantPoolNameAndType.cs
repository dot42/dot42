namespace Dot42.JvmClassLib.Structures
{
    internal sealed class ConstantPoolNameAndType : ConstantPoolEntry
    {
        private readonly int nameIndex;
        private readonly int descriptorIndex;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ConstantPoolNameAndType(ConstantPool constantPool, int nameIndex, int descriptorIndex)
            : base(constantPool)
        {
            this.nameIndex = nameIndex;
            this.descriptorIndex = descriptorIndex;
        }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name
        {
            get { return GetEntry<ConstantPoolUtf8>(nameIndex).Value; }
        }

        /// <summary>
        /// Gets the descriptor
        /// </summary>
        public string Descriptor
        {
            get { return GetEntry<ConstantPoolUtf8>(descriptorIndex).Value; }
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.NameAndType; }
        }

        /// <summary>
        /// Gets human readable string
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
