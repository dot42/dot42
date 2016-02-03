using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public class ArrayType : CompositeType
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ArrayType(TypeReference elementType)
            : this()
        {
            ElementType = elementType;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ArrayType()
        {
            TypeDescriptor = TypeDescriptors.Array;
        }

        /// <summary>
        /// Gets the type of each element in this array.
        /// </summary>
        public TypeReference ElementType { get; internal set; }

        /// <summary>
        /// Get this type reference in descriptor format.
        /// </summary>
        public override string Descriptor
        {
            get { return "[" + ElementType.Descriptor; }
        }

        public override string ToString()
        {
            return string.Concat("[", ElementType.ToString(), "]");
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(ArrayType other)
        {
            return base.Equals(other) && ElementType.Equals(other.ElementType);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public override bool Equals(TypeReference other)
        {
            return Equals(other as ArrayType);
        }

        public override bool Freeze()
        {
            bool gotFrozen = base.Freeze();
            if (gotFrozen)
                ElementType.Freeze();
            return gotFrozen;
        }

        public override bool Unfreeze()
        {
            bool thawed = base.Unfreeze();
            if (thawed)
                ElementType.Unfreeze();
            return thawed;
        }
    }
}