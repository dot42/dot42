namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolString : ConstantPoolEntry, IConstantPoolValue
    {
        private readonly int stringIndex;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolString(ConstantPool constantPool, int stringIndex)
            : base(constantPool)
        {
            this.stringIndex = stringIndex;
        }

        /// <summary>
        /// Gets the string's value
        /// </summary>
        public string Value
        {
            get { return GetEntry<ConstantPoolUtf8>(stringIndex).Value; }
        }

        /// <summary>
        /// Gets the actual value
        /// </summary>
        object IConstantPoolValue.Value { get { return Value; } }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.String; }
        }

        /// <summary>
        /// Gets human readable string
        /// </summary>
        public override string ToString()
        {
            return Value;
        }
    }
}
