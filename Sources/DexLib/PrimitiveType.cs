using System.Globalization;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib
{
    public sealed class PrimitiveType : TypeReference
    {
        public static readonly PrimitiveType Void = new PrimitiveType(TypeDescriptors.Void);
        public static readonly PrimitiveType Boolean = new PrimitiveType(TypeDescriptors.Boolean);
        public static readonly PrimitiveType Byte = new PrimitiveType(TypeDescriptors.Byte);
        public static readonly PrimitiveType Short = new PrimitiveType(TypeDescriptors.Short);
        public static readonly PrimitiveType Char = new PrimitiveType(TypeDescriptors.Char);
        public static readonly PrimitiveType Int = new PrimitiveType(TypeDescriptors.Int);
        public static readonly PrimitiveType Long = new PrimitiveType(TypeDescriptors.Long);
        public static readonly PrimitiveType Float = new PrimitiveType(TypeDescriptors.Float);
        public static readonly PrimitiveType Double = new PrimitiveType(TypeDescriptors.Double);

        /// <summary>
        /// Default ctor
        /// </summary>
        private PrimitiveType(TypeDescriptors typeDescriptor)
        {
            TypeDescriptor = typeDescriptor;
        }

        public override string ToString()
        {
            return ((char) TypeDescriptor).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get this type reference in descriptor format.
        /// </summary>
        public override string Descriptor
        {
            get { return ((char)TypeDescriptor).ToString(CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public bool Equals(PrimitiveType other)
        {
            return (other != null) && (TypeDescriptor == other.TypeDescriptor);
        }

        /// <summary>
        /// Is other equal to this?
        /// </summary>
        public override bool Equals(TypeReference other)
        {
            return Equals(other as PrimitiveType);
        }

        /// <summary>
        /// Does this type occupy 2 registers?
        /// </summary>
        public bool IsWide
        {
            get
            {
                switch (TypeDescriptor)
                {
                    case TypeDescriptors.Long:
                    case TypeDescriptors.Double:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}