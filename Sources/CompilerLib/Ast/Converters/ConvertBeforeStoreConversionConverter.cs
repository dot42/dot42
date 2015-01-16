using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert types before they are stored.
    /// </summary>
    internal static class ConvertBeforeStoreConversionConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions())
            {
                switch (node.Code)
                {
                    case AstCode.Stloc:
                        {
                            var variable = (AstVariable)node.Operand;
                            var varType = variable.Type;
                            var arg = node.Arguments[0];
                            ConvertIfNeeded(arg, varType);                            
                        }
                        break;
                    case AstCode.Stelem_I2:
                        {
                            var arrayElementType = node.Arguments[0].GetResultType().ElementType;
                            var arg = node.Arguments[2];
                            ConvertIfNeeded(arg, arrayElementType);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Convert the given argument if it does not match the target type.
        /// </summary>
        private static void ConvertIfNeeded(AstExpression arg, XTypeReference targetType)
        {
            var argType = arg.GetResultType();
            if (!targetType.IsSame(argType))
            {
                if (targetType.IsChar())
                {
                    if (argType.IsUInt16() || argType.IsByte()) Convert(arg, AstCode.Conv_U2, targetType);
                }
                else if (targetType.IsUInt16())
                {
                    if (argType.IsChar() || argType.IsByte()) Convert(arg, AstCode.Int_to_ushort, targetType);
                }
                else if (targetType.IsInt16())
                {
                    if (argType.IsChar() || argType.IsByte()) Convert(arg, AstCode.Conv_I2, targetType);
                }
            }
        }

        /// <summary>
        /// Convert the result of the given node to uint8/uint16 if needed.
        /// </summary>
        private static void Convert(AstExpression node, AstCode convertCode, XTypeReference expectedType)
        {
            // Copy load expression
            var clone = new AstExpression(node);

            // Convert node
            node.Code = convertCode;
            node.SetArguments(clone);
            node.Operand = null;
            node.SetType(expectedType);
        }
    }
}
