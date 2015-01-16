using Dot42.DexLib;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Dex related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Is the given type long or double?
        /// </summary>
        internal static bool IsWide(this TypeReference type)
        {
            return PrimitiveType.Long.Equals(type) || PrimitiveType.Double.Equals(type);
        }

        /// <summary>
        /// Is the given type a primitive type?
        /// </summary>
        internal static bool IsPrimitive(this TypeReference type)
        {
            return type is PrimitiveType;
        }

        /// <summary>
        /// Is the given type a (primitive) Float?
        /// </summary>
        internal static bool IsFloat(this TypeReference type)
        {
            return PrimitiveType.Float.Equals(type);
        }

        /// <summary>
        /// Is the given type a (primitive) Double?
        /// </summary>
        internal static bool IsDouble(this TypeReference type)
        {
            return PrimitiveType.Double.Equals(type);
        }
    }
}
