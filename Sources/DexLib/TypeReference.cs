using System;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public abstract class TypeReference : FreezableBase, IEquatable<TypeReference>
    {
        private TypeDescriptors _typeDescriptor;
        internal TypeDescriptors TypeDescriptor { get { return _typeDescriptor; } set { ThrowIfFrozen(); _typeDescriptor = value; } }
        private string _typeDescriptorCache;

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public override bool Equals(object other)
        {
            return Equals(other as TypeReference);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public virtual bool Equals(TypeReference other)
        {
            return (other != null) && (TypeDescriptor == other.TypeDescriptor);
        }

        /// <summary>
        /// Create a hashcode.
        /// </summary>
        public override int GetHashCode()
        {
            return TypeDescriptor.GetHashCode();
        }

        /// <summary>
        /// Get this type reference in descriptor format.
        /// </summary>
        public abstract string Descriptor { get; }

        public override bool Unfreeze()
        {
            bool thawed = base.Unfreeze();
            if (thawed)
                _typeDescriptorCache = null;
            return thawed;
        }

        internal string CachedTypeDescriptor
        {
            get { return IsFrozen ? _typeDescriptorCache : null; }
            set { if(IsFrozen) _typeDescriptorCache = value; }
        }
    }
}