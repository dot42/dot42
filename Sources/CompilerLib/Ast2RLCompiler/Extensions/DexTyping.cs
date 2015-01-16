using Dot42.CompilerLib.XModel;

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
        public static bool IsDexWide(this XTypeReference type)
        {
            // Check most likely case
            return type.IsWide();
        }

        /// <summary>
        /// Does the type require a non-object register in Dex?
        /// </summary>
        public static bool IsDexValue(this XTypeReference type)
        {
            // Check most likely case
            if (type.Is(XTypeReferenceKind.Byte, XTypeReferenceKind.SByte, XTypeReferenceKind.Bool, XTypeReferenceKind.Char,
                        XTypeReferenceKind.Short, XTypeReferenceKind.UShort,
                        XTypeReferenceKind.Int, XTypeReferenceKind.UInt, XTypeReferenceKind.Float,
                        XTypeReferenceKind.IntPtr, XTypeReferenceKind.UIntPtr))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Does the type require an object register in Dex?
        /// </summary>
        public static bool IsDexObject(this XTypeReference type)
        {
            return (type != null) && !type.IsPrimitive;
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexBoolean(this XTypeReference type)
        {
            return type.Is(XTypeReferenceKind.Bool);
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexByte(this XTypeReference type)
        {
            return type.Is(XTypeReferenceKind.Byte, XTypeReferenceKind.SByte);
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexChar(this XTypeReference type)
        {
            return type.Is(XTypeReferenceKind.Char);
        }

        /// <summary>
        /// Does the type require a boolean instruction in Dex?
        /// </summary>
        public static bool IsDexShort(this XTypeReference type)
        {
            return type.Is(XTypeReferenceKind.Short, XTypeReferenceKind.UShort);
        }
    }
}
