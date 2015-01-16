using Dot42.CompilerLib.XModel;

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
        public static void Convert(AstNode ast)
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
                            var clone = new AstExpression(node);
                            node.SetCode(AstCode.UnboxFromGeneric).SetArguments(clone).Operand = field.FieldType;
                        }
                        break;
                    case AstCode.Call:
                    case AstCode.Calli:
                    case AstCode.Callvirt:
                        {
                            var method = (XMethodReference)node.Operand;
                            if ((!method.ReturnType.IsVoid()) && (pair.Parent != null))
                            {
                                var clone = new AstExpression(node);
                                node.SetCode(AstCode.UnboxFromGeneric).SetArguments(clone).Operand = method.ReturnType;
                            }
                        }
                        break;
                }
            }
        }
    }
}
