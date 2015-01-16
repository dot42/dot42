using Dot42.JvmClassLib;

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
            if (type.IsBaseType)
            {
                var baseType = (BaseTypeReference) type;
                return (baseType.Type == BaseTypes.Double) || (baseType.Type == BaseTypes.Long);
            }
            return false;
        }
    
        /// <summary>
        /// Does the type require a non-object register in Dex?
        /// </summary>
        public static bool IsDexValue(this TypeReference type)
        {
            if (type.IsBaseType)
            {
                var baseType = (BaseTypeReference)type;
                var xtype = baseType.Type;
                return (xtype == BaseTypes.Boolean) || (xtype == BaseTypes.Byte) || (xtype == BaseTypes.Char) || (xtype == BaseTypes.Int) || (xtype == BaseTypes.Short) || (xtype == BaseTypes.Float);
            }
            return false;
        }

        /// <summary>
        /// Does the type require an object register in Dex?
        /// </summary>
        public static bool IsDexObject(this TypeReference type)
        {
            return (type != null) && !type.IsBaseType;
        }
    }
}
