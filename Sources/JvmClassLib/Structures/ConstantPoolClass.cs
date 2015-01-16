using System.Diagnostics;

namespace Dot42.JvmClassLib.Structures
{
    public sealed class ConstantPoolClass : ConstantPoolEntry
    {
        private readonly int nameIndex;
        private ClassFile resolved;
        private TypeReference typeReference;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolClass(ConstantPool constantPool, int nameIndex)
            : base(constantPool)
        {
            this.nameIndex = nameIndex;
        }

        /// <summary>
        /// Gets the class name
        /// </summary>
        public string Name
        {
            get { return GetEntry<ConstantPoolUtf8>(nameIndex).Value; }
        }

        /// <summary>
        /// Gets the class name as type reference
        /// </summary>
        public TypeReference Type
        {
            get { return  typeReference ?? (typeReference = Parse(Name)); }
        }

        /// <summary>
        /// Parse the given descriptor into a type reference.
        /// </summary>
        private static TypeReference Parse(string descriptor)
        {
            if (descriptor.StartsWith("["))
                return new ArrayTypeReference(Parse(descriptor.Substring(1)));
            if (descriptor.StartsWith("L") && descriptor.EndsWith(";"))
                return Parse(descriptor.Substring(1, descriptor.Length - 2));
            BaseTypeReference baseType;
            if ((descriptor.Length == 1) && BaseTypeReference.TryGetByCode(descriptor[0], out baseType))
                return baseType;
            return new ObjectTypeReference(descriptor, null);
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Class; }
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        public bool TryResolve(out ClassFile result)
        {
            if (resolved == null)
            {
                var type = Type as ObjectTypeReference;
                if (type != null)
                {
                    Loader.TryLoadClass(Name, out resolved);
                }
            }
            result = resolved;
            return (result != null);
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
