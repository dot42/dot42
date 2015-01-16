using Dot42.CecilExtensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.Ast2RLCompiler.Extensions
{
    /// <summary>
    /// Determined properties of types.
    /// </summary>
    partial class ILCompilerExtensions
    {
        /// <summary>
        /// Does the type require a wide register set in Dex?
        /// </summary>
        public static bool IsDexWide(this TypeReference type)
        {
            // Check most likely case
            if (type.Is(MetadataType.Int64, MetadataType.Double, MetadataType.UInt64))
            {
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Does the type require a non-object register in Dex?
        /// </summary>
        public static bool IsDexValue(this TypeReference type)
        {
            // Check most likely case
            if (type.Is(MetadataType.Byte, MetadataType.SByte, MetadataType.Boolean, MetadataType.Char,
                        MetadataType.Int16, MetadataType.UInt16,
                        MetadataType.Int32, MetadataType.UInt32, MetadataType.Single))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Does the type require an object register in Dex?
        /// </summary>
        public static bool IsDexObject(this TypeReference type)
        {
            return (type != null) && !type.IsPrimitive;
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexBoolean(this TypeReference type)
        {
            return type.Is(MetadataType.Boolean);
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexByte(this TypeReference type)
        {
            return type.Is(MetadataType.Byte, MetadataType.SByte);
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexChar(this TypeReference type)
        {
            return type.Is(MetadataType.Char, MetadataType.UInt16);
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexShort(this TypeReference type)
        {
            return type.Is(MetadataType.Int16);
        }
    }
}
