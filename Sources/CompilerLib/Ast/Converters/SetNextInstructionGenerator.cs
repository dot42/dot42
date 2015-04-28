using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dot42.CompilerLib.XModel;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Generate code to support SetNextInstruction in Debugger.
    /// 
    /// For the initial implementation, we only support going to the
    /// start of the method. Later the implementation can be expanded.
    /// 
    /// To go to an arbitrary location, the inserted branch would go to
    /// the end of the method, where a switch statement would redirect
    /// to the final destination. care - and maybe debugger support - 
    /// would be needed to initialize all variables correctly, if they haven't
    /// been already; depending on source and target.
    /// 
    /// </summary>
    internal static class SetNextInstructionGenerator 
    {
        public static void Convert(AstBlock ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            var setInstructionTarget = new AstGeneratedVariable(DebuggerConstants.SetNextInstructionVariableName, null, true)
            {
                Type = compiler.Module.TypeSystem.Bool
            };

            int labelCount = 0;
            var initExpr = new AstExpression(AstNode.NoSource, AstCode.Stloc, setInstructionTarget,
                                                    new AstExpression(AstNode.NoSource, AstCode.Ldnull, null)
                                                             .SetType(compiler.Module.TypeSystem.Int));

            int lastBaseCtorCall = -1;

            // can't jump before base-class constructor call.
            if (currentMethod.IsCtor)
                lastBaseCtorCall = FindLastCtorCall(ast);

            ISourceLocation currentLoc = null;
            
            foreach (var block in ast.GetSelfAndChildrenRecursive<AstBlock>())
            {
                if (block.EntryGoto != null) // only handle simple cases atm.
                    return;

                var body = block.Body;
                AstLabel label = null;
                int firstValidExpression = -1;
                //bool lastExprWasRet = false;

                var startIdx = lastBaseCtorCall == -1?0:lastBaseCtorCall + 1;
                lastBaseCtorCall = -1;

                for (int idx = startIdx; idx < body.Count; ++idx)
                {
                    var expr = body[idx] as AstExpression;
                    if(expr == null)
                        continue;

                    if (expr.SourceLocation == null || expr.SourceLocation.Equals(currentLoc))
                        continue;

                    currentLoc = expr.SourceLocation;

                    //lastExprWasRet = expr.Code == AstCode.Ret;

                    if (firstValidExpression == -1)
                    {
                        firstValidExpression = idx;
                        continue;
                    }

                    AddJumpInstruction(body, currentLoc, setInstructionTarget, ref idx, 
                                       ref label, firstValidExpression, initExpr, ref labelCount);
                }

                //if (!lastExprWasRet && firstValidExpression != -1)
                //{
                //    int idx = body.Count;
                //    AddJumpInstruction(body, currentLoc, setInstructionTarget, ref idx,
                //                       ref label, firstValidExpression, initExpr, ref labelCount);
                //}
            }
        }

        private static int FindLastCtorCall(AstBlock block)
        {
            for (int idx = block.Body.Count -1; idx >= 0; --idx)
            {
                var expr = block.Body[idx] as AstExpression;
                if (expr == null)
                    continue;
                if (expr.Code == AstCode.CallBaseCtor)
                    return idx;
                if (expr.Code.IsCall())
                {
                    var method = (XMethodReference)expr.Operand;
                    if (method.Resolve().IsConstructor)
                        return idx;
                }
            }
            return -1;
        }

        private static void AddJumpInstruction(List<AstNode> body, ISourceLocation currentLoc, 
                                               AstGeneratedVariable setInstructionTarget, ref int idx,
                                               ref AstLabel label, int firstValidExpression, AstExpression initExpr, 
                                               ref int labelCount)
        {
            if (label == null)
            {
                label = new AstLabel(AstNode.NoSource, "setInstructionTarget_" + (++labelCount));
                body.Insert(firstValidExpression, label);
                body.Insert(firstValidExpression + 1, initExpr);
                idx += 2;
            }

            var branch = new AstExpression(currentLoc, AstCode.Brtrue, label,
                                new AstExpression(currentLoc, AstCode.Ldloc, setInstructionTarget));
            body.Insert(idx, branch);
            idx += 1;
        }
    }
}
