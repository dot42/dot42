using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Add/expand generic unboxing
    /// </summary>
    internal static class GenericsConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, MethodSource currentMethod)
        {
            // Expand typeof
            foreach (var pair in ast.GetExpressionPairs())
            {
                var node = pair.Expression;
                switch (node.Code)
                {
                    case AstCode.Ldfld:
                    //case AstCode.Ldsfld: // NOT YET
                        {
                            var field = (XFieldReference) node.Operand;
                            UnboxIfGeneric(field.FieldType, node);
                        }
                        break;
                    case AstCode.Stfld:
                        {
                            var field = (XFieldReference)node.Operand;
                            BoxIfGeneric(field.FieldType, node.Arguments[1]);
                        }
                        break;
                    case AstCode.Call:
                    case AstCode.Calli:
                    case AstCode.Callvirt:
                        {
                            var method = (XMethodReference)node.Operand;
                            if ((!method.ReturnType.IsVoid()) && (pair.Parent != null))
                            {
                                UnboxIfGeneric(method.ReturnType, node);
                            }
                        }
                        break;
                    case AstCode.Ret:
                        {
                            if (node.Arguments.Count > 0)
                            {
                                var expectedType = currentMethod.Method.ReturnType;
                                BoxIfGeneric(expectedType, node.Arguments[0]);
                            }
                        }
                        break;
                }
            }
        }

        private static void UnboxIfGeneric(XTypeReference type, AstExpression node)
        {
            if (type.IsGenericParameter || type.IsGenericParameterArray())
            {
                var resultType = node.GetResultType();
                if (!type.IsByReference && resultType.IsByReference)
                {
                    var elementType = resultType.ElementType;

                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.SimpleCastclass).SetArguments(clone).Operand = elementType;
                }
                else
                {
                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.UnboxFromGeneric).SetArguments(clone).Operand = type;
                }
            }
        }

        private static void BoxIfGeneric(XTypeReference type, AstExpression node)
        {
            // TODO: CLR allows return-by-reference, though C# does not. Do we need to handle this here?

            if (type.IsGenericParameter)
            {
                var resultType = node.GetResultType();
                if (resultType.IsPrimitive)
                {
                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.BoxToGeneric)
                                .SetArguments(clone)
                                .Operand = type;
                }
            }
            else if (type.IsGenericParameterArray())
            {
                var resultType = node.GetResultType().ElementType;
                if (resultType.IsPrimitive)
                {
                    var clone = new AstExpression(node);
                    node.SetCode(AstCode.BoxToGeneric).SetArguments(clone).Operand = type;
                }
            }
        }
    }
}
