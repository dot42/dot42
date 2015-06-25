using System;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    enum PullTarget
    {
        Branch,
        Comparison,
    }
    /// <summary>
    /// Optimize various branch situations
    /// </summary>
    internal static class BranchOptimizer2 
    {
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            // combine ceq/cne (cxx(x,x), 0/null/false/true)  expressions
            var booleanType = compiler.Module.TypeSystem.Bool;
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.Ceq || x.Code == AstCode.Cne)).Reverse())
            {
                var reverse = InvertComparisonIfPullComparisonUpToEquals(node, PullTarget.Comparison);

                if (reverse == null)
                    continue;

                if (node.Code == AstCode.Cne)
                    reverse = !reverse;

                var expr = node.Arguments[0];

                AstCode code = reverse.Value ? ReverseCode(expr.Code, expr.Arguments[0].GetResultType()) : expr.Code;

                var newExpr = new AstExpression(expr.SourceLocation, code, node.Operand, expr.Arguments[0], expr.Arguments[1])
                                    .SetType(booleanType);
                                        
                node.CopyFrom(newExpr, true);
            }

            // Convert beq/bne (cxx(x,x), 0/null/false/true) expressions to brtrue/brfalse
            ConvertBeqBneWithConstToBrtrueBrfalse(ast);

            // combine brtrue/brfalse (cxx(x,x)) expressions
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.Brtrue) || (x.Code == AstCode.Brfalse)))
            {
                var expr = node.Arguments[0];
                if (expr.Code.IsCompare() && (expr.Arguments.Count == 2))
                {
                    var arg1 = expr.Arguments[0];
                    var arg2 = expr.Arguments[1];

                    var arg1Type = arg1.GetResultType();
                    var arg2Type = arg2.GetResultType();
                    if (CanPullComparisonUp(node.Code, arg1Type, arg2Type, PullTarget.Branch))
                    {
                        // Simplify
                        var code = (node.Code == AstCode.Brtrue) ? expr.Code : expr.Code.Reverse();

                        var newExpr = new AstExpression(expr.SourceLocation, code.ToBranch(), node.Operand, arg1, arg2);
                        node.CopyFrom(newExpr, true);
                    }
                    else if (CanPullComparisonUp(node.Code, arg1Type, arg2Type, PullTarget.Comparison))
                    {
                        // must be double/long/float argType
                        // simplify
                        var code = (node.Code == AstCode.Brtrue) ? expr.Code : ReverseCode(expr.Code, arg1Type);
                        bool needLBias = code == AstCode.Cgt || code == AstCode.Cge || code == AstCode.Cle_Un || code == AstCode.Clt_Un;
                        
                        var cmpCode = arg1Type.IsFloat() || arg1Type.IsDouble() 
                                        ? (needLBias? AstCode.CmpLFloat : AstCode.CmpGFloat)
                                        : AstCode.CmpLong;

                        var newExpr = new AstExpression(expr.SourceLocation, code.ToBranchZ(), node.Operand,
                                                new AstExpression(expr.SourceLocation, cmpCode, null, arg1, arg2));
                        node.CopyFrom(newExpr, true);
                    }
                }
            }

            // Convert beq/bne (cxx(x,x), 0/null/false/true) expressions to brtrue/brfalse
            // again.
            ConvertBeqBneWithConstToBrtrueBrfalse(ast);
        }

        private static void ConvertBeqBneWithConstToBrtrueBrfalse(AstNode ast)
        {
            // Convert beq/bne (cxx(x,x), 0/null/false/true) expressions to brtrue/brfalse
            foreach (var node in ast.GetExpressions(x => (x.Code == AstCode.__Beq || x.Code == AstCode.__Bne_Un)).Reverse())
            {
                bool? reverse = ReverseCodeOnEqualCollapse(node);

                if (reverse == null)
                    continue;

                var expr = node.Arguments[0];

                if (node.Code == AstCode.__Bne_Un)
                    reverse = !reverse;

                var code = reverse.Value ? AstCode.Brfalse : AstCode.Brtrue;

                var newExpr = new AstExpression(expr.SourceLocation, code, node.Operand, expr);
                node.CopyFrom(newExpr, true);
            }
        }

        /// <summary>
        /// Reverses the code, taking account float/double NaN comparisons
        /// </summary>
        private static AstCode ReverseCode(AstCode code, XTypeReference type)
        {
            bool isFlt = type.IsDouble() || type.IsFloat();

            if (!isFlt)
                return code.Reverse();

            switch (code)
            {
                case AstCode.Ceq:
                    return AstCode.Cne;
                case AstCode.Cne:
                    return AstCode.Ceq;
                case AstCode.Cle:
                    return AstCode.Cgt_Un;
                case AstCode.Cle_Un:
                    return AstCode.Cgt;
                case AstCode.Clt:
                    return AstCode.Cge_Un;
                case AstCode.Clt_Un:
                    return AstCode.Cge;
                case AstCode.Cgt:
                    return AstCode.Cle_Un;
                case AstCode.Cgt_Un:
                    return AstCode.Cle;
                case AstCode.Cge:
                    return AstCode.Clt_Un;
                case AstCode.Cge_Un:
                    return AstCode.Clt;
                default:
                    throw new ArgumentOutOfRangeException("code", code.ToString());
            }            
        }

        private static bool? InvertComparisonIfPullComparisonUpToEquals(AstExpression node, PullTarget target)
        {
            bool? reverse = ReverseCodeOnEqualCollapse(node);
            if (reverse == null)
                return null;

            var expr = node.Arguments[0];

            if (!expr.Code.IsCompare() || expr.Arguments.Count != 2)
                return null;

            var arg1 = expr.Arguments[0];
            var arg2 = expr.Arguments[1];

            if (!CanPullComparisonUp(expr.Code, arg1.GetResultType(), arg2.GetResultType(), target))
                return null;

            return reverse;
        }

        private static bool? ReverseCodeOnEqualCollapse(AstExpression node)
        {
            if (node.Arguments.Count != 2)
                return null;

            var constExpr = node.Arguments[1];
            bool isBool = node.Arguments[0].IsBoolean();

            if (constExpr.Code == AstCode.Ldc_I4)
            {
                int operand = (int) constExpr.Operand;
                if(operand == 0)
                    return true;
                if (isBool && operand == 1)
                    return false;
            }

            if (constExpr.Code == AstCode.Ldc_I8 && (long) constExpr.Operand == 0)
                return true;

            if (constExpr.Code == AstCode.Ldnull)
                return true;

            return null ;
        }

        private static bool CanPullComparisonUp(AstCode code, XTypeReference arg1, XTypeReference arg2, PullTarget target)
        {
            if (arg1 == null || arg2 == null)
                return false;

            bool isReference = arg1.IsDexObject() && arg1.IsDexObject();

            if (!isReference && !arg1.IsSame(arg2))
                return false;

            if (target == PullTarget.Comparison)
                return true;

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
