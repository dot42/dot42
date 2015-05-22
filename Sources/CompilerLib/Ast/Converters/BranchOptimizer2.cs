using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Optimize various branch situations
    /// </summary>
    internal static class BranchOptimizer2 
    {
        public static void Convert(AstNode ast)
        {
            // Optimize brtrue (cxx(x,x)) expressions
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.Brtrue) || (x.Code == AstCode.Brfalse)))
            {
                var expr = node.Arguments[0];
                if (expr.Code.IsCompare() && (expr.Arguments.Count == 2))
                {
                    var arg1 = expr.Arguments[0];
                    var arg2 = expr.Arguments[1];

                    if (!CanPullComparisonUp(node.Code, arg1.GetResultType(), arg2.GetResultType())) 
                        continue;

                    // Simplify
                    var code = (node.Code == AstCode.Brtrue) ? expr.Code : expr.Code.Reverse();
                    var newExpr = new AstExpression(node.SourceLocation, code.ToBranch(), node.Operand, arg1, arg2);
                    node.CopyFrom(newExpr);
                }
            }

            // Optimize ceq (cxx(x,x)) expressions
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.Ceq)).Reverse())
            {
                var reverse = InvertComparisonIfPullComparisonUpOnEquals(node);

                if (reverse == null)
                    continue;

                var expr = node.Arguments[0];
                var code = reverse.Value ? expr.Code.Reverse() : expr.Code;

                var newExpr = new AstExpression(node.SourceLocation, code, node.Operand, expr.Arguments[0], expr.Arguments[1]);
                node.CopyFrom(newExpr);
            }

            // Optimize beq (cxx(x,x)) expressions
            // todo check if also  x.Code==AstCode.BrIfEq could apply
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.__Beq)).Reverse())
            {
                var reverse = InvertComparisonIfPullComparisonUpOnEquals(node);

                if (reverse == null)
                    continue;

                var expr = node.Arguments[0];
                var code = reverse.Value ? expr.Code.Reverse() : expr.Code;

                var newExpr = new AstExpression(node.SourceLocation, code.ToBranch(), node.Operand, expr.Arguments[0], expr.Arguments[1]);
                node.CopyFrom(newExpr);

            }

            // Optimize beq/bne (x, 0/null)) expressions
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.__Beq || x.Code == AstCode.__Bne_Un)).Reverse())
            {
                bool? reverse = ReverseCodeOnEqualCollapse(node);
                if (reverse != true) 
                    continue;

                // comparing with 0 or null.
                var expr = node.Arguments[0];
                if (expr.IsBoolean() || expr.IsInt32() || expr.GetResultType().IsDexObject())
                {
                    node.Code = node.Code == AstCode.__Beq ? AstCode.Brfalse : AstCode.Brtrue;
                    node.Arguments.RemoveAt(1);
                }
            }
        }

        private static bool? InvertComparisonIfPullComparisonUpOnEquals(AstExpression node)
        {
            bool? reverse = ReverseCodeOnEqualCollapse(node);
            if (reverse == null)
                return null;

            var expr = node.Arguments[0];

            if (!expr.Code.IsCompare() || expr.Arguments.Count != 2)
                return null;

            var arg1 = expr.Arguments[0];
            var arg2 = expr.Arguments[1];

            if (!CanPullComparisonUp(expr.Code, arg1.GetResultType(), arg2.GetResultType()))
                return null;

            return reverse;
        }

        private static bool? ReverseCodeOnEqualCollapse(AstExpression node)
        {
            if (node.Arguments.Count != 2)
                return null;

            var constExpr = node.Arguments[1];

            if (constExpr.Code == AstCode.Ldc_I4)
            {
                switch (((int) constExpr.Operand))
                {
                    case 0:
                        return true;
                    case 1:
                        return false;
                }
            }

            if (constExpr.Code == AstCode.Ldc_I8)
            {
                switch (((long)constExpr.Operand))
                {
                    case 0:
                        return true;
                    case 1:
                        return false;
                }
            }

            if (constExpr.Code == AstCode.Ldnull)
                return true;

            return null ;
        }

        private static bool CanPullComparisonUp(AstCode code, XTypeReference arg1, XTypeReference arg2)
        {
            if (arg1 == null || arg2 == null)
                return false;

            bool isReference = arg1.IsDexObject() && arg1.IsDexObject();
            if (!isReference && !arg1.IsSame(arg2))
                return false;

            if (arg1.Is(XTypeReferenceKind.Float))
                return false;
            if (arg1.IsDexWide())
                return false;

            bool isEq = IsEqualsBranchOrComparison(code);

            if (isEq)
                return true;

            
            bool isUnsigned = arg1.IsUInt16() || arg1.IsUInt32(); // TODO: check if we really have to exclude unsigned.

            if (isReference || isUnsigned)
                return false;

            return true;
        }

        private static bool IsEqualsBranchOrComparison(AstCode code)
        {
            return (code == AstCode.Ceq     || code == AstCode.Cne 
                 || code == AstCode.Brtrue  || code == AstCode.Brfalse
                 || code == AstCode.BrIfNe  || code == AstCode.BrIfEq
                 || code == AstCode.__Beq   || code == AstCode.__Beq
                 || code == AstCode.__Bne_Un|| code == AstCode.__Bne_Un);
        }
    }
}
