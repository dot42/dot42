namespace Dot42.JvmClassLib.Structures
{
    public abstract class ConstantPoolMemberRef : ConstantPoolEntry
    {
        private readonly int classIndex;
        private readonly int nameAndTypeIndex;
        private ClassFile resolvedClass;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolMemberRef(ConstantPool constantPool, int classIndex, int nameAndTypeIndex)
            : base(constantPool)
        {
            this.classIndex = classIndex;
            this.nameAndTypeIndex = nameAndTypeIndex;
        }

        /// <summary>
        /// Gets the members class name
        /// </summary>
        public string ClassName
        {
            get { return GetEntry<ConstantPoolClass>(classIndex).Name; }
        }

        /// <summary>
        /// Gets the members declaring class
        /// </summary>
        public TypeReference DeclaringType
        {
            get { return GetEntry<ConstantPoolClass>(classIndex).Type; }
        }

        /// <summary>
        /// Gets the members name
        /// </summary>
        public string Name
        {
            get { return GetEntry<ConstantPoolNameAndType>(nameAndTypeIndex).Name; }
        }

        /// <summary>
        /// Gets the members descriptor
        /// </summary>
        public string Descriptor
        {
            get { return GetEntry<ConstantPoolNameAndType>(nameAndTypeIndex).Descriptor; }
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        public bool TryResolveClass(out ClassFile result)
        {
            if (resolvedClass == null)
            {
                DoTryResolveClass(out resolvedClass);
            }
            result = resolvedClass;
            return (result != null);
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        private bool DoTryResolveClass(out ClassFile result)
        {
            if (ClassName[0] == '[')
                return Loader.TryLoadClass("java/lang/Object", out result);
            return Loader.TryLoadClass(ClassName, out result);
        }

        /// <summary>
        /// Gets human readable string
        /// </summary>
        public override string ToString()
        {
            return ClassName + "." + Name + " " + Descriptor;
        }
    }
}
