using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ubyte/ushort after they are loaded from signed to unsigned.
    /// </summary>
    internal static class ConvertAfterLoadConversionConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var pair in ast.GetExpressionPairs())
            {
                var node = pair.Expression;
                switch (node.Code)
                {
                    case AstCode.Ldc_I4:
                        {
                            var constType = node.GetResultType();
                            if (constType.IsByte())
                            {
                                node.Operand = System.Convert.ToInt32(node.Operand) & 0xFF;
                            }
                            else if (constType.IsUInt16())
                            {
                                node.Operand = System.Convert.ToInt32(node.Operand) & 0xFFFF;
                            }
                        }
                        break;
                    case AstCode.Ldloc:
                        {
                            var variable = (AstVariable) node.Operand;
                            var varType = variable.Type;
                            ConvertIfNeeded(node, varType, pair.Parent);
                        }
                        break;
                    case AstCode.Ldfld:
                    case AstCode.Ldsfld:
                        {
                            var field = (XFieldReference) node.Operand;
                            var fieldType = field.FieldType;
                            ConvertIfNeeded(node, fieldType, pair.Parent);
                        }
                        break;
                    case AstCode.Ldelem_U1:
                    case AstCode.Ldelem_U2:
                        {
                            var arrayType = (XArrayType) node.Arguments[0].GetResultType();
                            var elementType = arrayType.ElementType;
                            ConvertIfNeeded(node, elementType, pair.Parent);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Convert the result of the given node to uint8/uint16 if needed.
        /// </summary>
        private static void ConvertIfNeeded(AstExpression node, XTypeReference valueType, AstExpression parent)
        {
            AstCode convCode;
            if (valueType.IsByte())
            {
                // Convert from int to uint8
                convCode = AstCode.Int_to_ubyte;
            }
            else if (valueType.IsUInt16())
            {
                // Convert from int to uint16
                convCode = AstCode.Int_to_ushort;
            }
            else if (valueType.IsChar())
            {
                // Convert from int to uint16
                convCode = AstCode.Conv_U2;
            }
            else
            {
                // Do not convert
                return;
            }

            // Avoid recursion
            if ((parent != null) && (parent.Code == convCode))
                return;

            // Copy load expression
            var clone = new AstExpression(node);

            // Convert node
            node.Code = convCode;
            node.Arguments.Clear();
            node.Arguments.Add(clone);
            node.Operand = null;
            node.ExpectedType = valueType;
            node.InferredType = valueType;
        }
    }
}
