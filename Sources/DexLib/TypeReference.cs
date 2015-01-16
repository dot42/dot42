using System;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public abstract class TypeReference : IEquatable<TypeReference>
    {
        internal TypeDescriptors TypeDescriptor { get; set; }

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
    }
}