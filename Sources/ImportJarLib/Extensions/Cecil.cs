using Mono.Cecil;

namespace Dot42.ImportJarLib.Extensions
{
    internal static partial class Cecil
    {

        /// <summary>
        /// Is the given type System.Void?
        /// </summary>
        public static bool IsVoid(this TypeReference type)
        {
            return type.Is(MetadataType.Void);
        }

        /// <summary>
        /// Is the given type System.Int64?
        /// </summary>
        public static bool Is(this TypeReference type, MetadataType expectedType)
        {
            return (type != null) && (type.MetadataType == expectedType);
        }
    }
}
