using Dot42.CompilerLib.Ast.Extensions;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Optimize only simple branch situations, as types may change after this step.
    /// </summary>
    internal static class BranchOptimizer 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            // Optimize brtrue (cxx(int, int)) expressions
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.Brtrue) || (x.Code == AstCode.Brfalse)))
            {
                var expr = node.Arguments[0];
                if (expr.Code.IsCompare() && (expr.Arguments.Count == 2))
                {
                    var arg1 = expr.Arguments[0];
                    var arg2 = expr.Arguments[1];

                    if (arg1.IsInt32() && arg2.IsInt32())
                    {
                        // Simplify
                        var code = (node.Code == AstCode.Brtrue) ? expr.Code : expr.Code.Reverse();
                        var newExpr = new AstExpression(expr.SourceLocation, code.ToBranch(), node.Operand, arg1, arg2);
                        node.CopyFrom(newExpr, true);
                    }
                }
            }
        }
    }
}
