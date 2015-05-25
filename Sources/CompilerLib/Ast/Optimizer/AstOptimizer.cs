using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Optimizer
{
    public partial class AstOptimizer
    {
        private int nextLabelIndex;
        private readonly DecompilerContext context;
        private readonly AstBlock method;

        public AstOptimizer(DecompilerContext context, AstBlock method)
        {
            this.context = context;
            this.method = method;            
        }

        public void Optimize(AstOptimizationStep abortBeforeStep = AstOptimizationStep.None)
        {
            if (abortBeforeStep == AstOptimizationStep.RemoveRedundantCode) return;
            RemoveRedundantCode(method);

            if (abortBeforeStep == AstOptimizationStep.ReduceBranchInstructionSet) return;
            foreach (AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                ReduceBranchInstructionSet(block);
            }
            // ReduceBranchInstructionSet runs before inlining because the non-aggressive inlining heuristic
            // looks at which type of instruction consumes the inlined variable.

            if (abortBeforeStep == AstOptimizationStep.InlineVariables) return;
            // Works better after simple goto removal because of the following debug pattern: stloc X; br Next; Next:; ldloc X
            var inlining1 = new AstInlining(method);
            inlining1.InlineAllVariables();

            if (abortBeforeStep == AstOptimizationStep.CopyPropagation) return;
            inlining1.CopyPropagation();

            if (abortBeforeStep == AstOptimizationStep.SplitToMovableBlocks) return;
            foreach (var block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                SplitToBasicBlocks(block);
            }

            if (abortBeforeStep == AstOptimizationStep.TypeInference) return;
            // Types are needed for the ternary operator optimization
            TypeAnalysis.Run(context, method);

            foreach (AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                bool modified;
                do
                {
                    modified = false;

                    if (abortBeforeStep == AstOptimizationStep.SimplifyNullCoalescing) return;
                    modified |= block.RunOptimization(new SimpleControlFlow(context, method).SimplifyNullCoalescing);

                    if (abortBeforeStep == AstOptimizationStep.JoinBasicBlocks) return;
                    modified |= block.RunOptimization(new SimpleControlFlow(context, method).JoinBasicBlocks);

                    if (abortBeforeStep == AstOptimizationStep.SimplifyShiftOperators) return;
                    modified |= block.RunOptimization(SimplifyShiftOperators);

                    if (abortBeforeStep == AstOptimizationStep.TransformDecimalCtorToConstant) return;
                    modified |= block.RunOptimization(TransformDecimalCtorToConstant);
                    modified |= block.RunOptimization(SimplifyLdcI4ConvI8);

                    if (abortBeforeStep == AstOptimizationStep.SimplifyLdObjAndStObj) return;
                    modified |= block.RunOptimization(SimplifyLdObjAndStObj);

                    if (abortBeforeStep == AstOptimizationStep.TransformArrayInitializers) return;
                    modified |= block.RunOptimization(TransformArrayInitializers);

                    if (abortBeforeStep == AstOptimizationStep.TransformMultidimensionalArrayInitializers) return;
                    modified |= block.RunOptimization(TransformMultidimensionalArrayInitializers);

                    if (abortBeforeStep == AstOptimizationStep.MakeAssignmentExpression) return;
                    modified |= block.RunOptimization(MakeAssignmentExpression);
#if COMPOUNDASSIGNMENT
                    modified |= block.RunOptimization(MakeCompoundAssignments);
#endif

#if POSTINCREMENT
                    if (abortBeforeStep == AstOptimizationStep.IntroducePostIncrement) return;
                    modified |= block.RunOptimization(IntroducePostIncrement);
#endif

                    if (abortBeforeStep == AstOptimizationStep.InlineExpressionTreeParameterDeclarations) return;
                    if (context.Settings.ExpressionTrees)
                    {
                        modified |= block.RunOptimization(InlineExpressionTreeParameterDeclarations);
                    }

                    if (abortBeforeStep == AstOptimizationStep.InlineVariables2) return;
                    modified |= new AstInlining(method).InlineAllInBlock(block);
                    new AstInlining(method).CopyPropagation();

                } while (modified);
            }

            /*if (abortBeforeStep == AstOptimizationStep.FindLoops) return;
            foreach (AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                new LoopsAndConditions(context).FindLoops(block);
            }
            */

            /*if (abortBeforeStep == AstOptimizationStep.FindConditions) return;
            foreach (AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                new LoopsAndConditions(context).FindConditions(block);
            }*/

            if (abortBeforeStep == AstOptimizationStep.FlattenNestedMovableBlocks) return;
            FlattenBasicBlocks(method);

            if (abortBeforeStep == AstOptimizationStep.RemoveEndFinally) return;
            RemoveEndFinally(method);

            if (abortBeforeStep == AstOptimizationStep.RemoveRedundantCode2) return;
            RemoveRedundantCode(method);

            if (abortBeforeStep == AstOptimizationStep.GotoRemoval) return;
            new GotoRemoval().RemoveGotos(method);

            if (abortBeforeStep == AstOptimizationStep.DuplicateReturns) return;
            DuplicateReturnStatements(method);

            if (abortBeforeStep == AstOptimizationStep.GotoRemoval2) return;
            new GotoRemoval().RemoveGotos(method);

            if (abortBeforeStep == AstOptimizationStep.InlineVariables3) return;
            // The 2nd inlining pass is necessary because DuplicateReturns and the introduction of ternary operators
            // open up additional inlining possibilities.
            new AstInlining(method).InlineAllVariables();

            if (abortBeforeStep == AstOptimizationStep.RecombineVariables) return;
            //RecombineVariables(method); // We do not recombine variables because the RL code depends on it.

            if (abortBeforeStep == AstOptimizationStep.TypeInference2) return;
            TypeAnalysis.Reset(method);
            TypeAnalysis.Run(context, method);

            if (abortBeforeStep == AstOptimizationStep.RemoveRedundantCode3) return;
            GotoRemoval.RemoveRedundantCode(method);

            // ReportUnassignedILRanges(method);
        }

        /// <summary>
        /// Removes redundatant Br, Nop, Dup, Pop
        /// Ignore arguments of 'leave'
        /// </summary>
        void RemoveRedundantCode(AstBlock method)
        {
            var labelRefCount = new Dictionary<AstLabel, int>();
            foreach (var target in method.GetSelfAndChildrenRecursive<AstExpression>(e => e.IsBranch()).SelectMany(e => e.GetBranchTargets()))
            {
                labelRefCount[target] = labelRefCount.GetOrDefault(target) + 1;
            }

            foreach (var block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                var body = block.Body;
                var newBody = new List<AstNode>(body.Count);
                for (var i = 0; i < body.Count; i++)
                {
                    AstLabel target;
                    AstExpression popExpr;
                    var node = body[i];
                    if (node.Match(AstCode.Br, out target) && i + 1 < body.Count && body[i + 1] == target)
                    {
                        // Ignore the branch
                        if (labelRefCount[target] == 1)
                            i++;  // Ignore the label as well
                    }
                    else if (node.Match(AstCode.Nop))
                    {
                        // Ignore nop
                    }
                    else if (node.Match(AstCode.Pop, out popExpr))
                    {
                        AstVariable v;
                        if (!popExpr.Match(AstCode.Ldloc, out v))
                            throw new Exception("Pop should have just ldloc at this stage");
                        // Best effort to move the ILRange to previous statement
                        AstVariable prevVar;
                        AstExpression prevExpr;
                        if (i - 1 >= 0 && body[i - 1].Match(AstCode.Stloc, out prevVar, out prevExpr) && prevVar == v)
                            prevExpr.ILRanges.AddRange(((AstExpression)body[i]).ILRanges);
                        // Ignore pop
                    }
                    else
                    {
                        newBody.Add(node);
                    }
                }
                block.Body = newBody;
            }

            // Ignore arguments of 'leave'
            foreach (var expr in method.GetSelfAndChildrenRecursive<AstExpression>(e => e.Code == AstCode.Leave))
            {
                if (expr.Arguments.Any(arg => !arg.Match(AstCode.Ldloc)))
                    throw new Exception("Leave should have just ldloc at this stage");
                expr.Arguments.Clear();
            }

            // 'dup' removal
            foreach (var expr in method.GetSelfAndChildrenRecursive<AstExpression>())
            {
                for (var i = 0; i < expr.Arguments.Count; i++)
                {
                    AstExpression child1;
                    var argi = expr.Arguments[i];
                    if (argi.Match(AstCode.Dup, out child1))
                    {
                        child1.ILRanges.AddRange(argi.ILRanges);
                        expr.Arguments[i] = child1;
                    }
                }
            }
        }

        /// <summary>
        /// Reduces the branch codes to just br and brtrue.
        /// Moves ILRanges to the branch argument
        /// </summary>
        void ReduceBranchInstructionSet(AstBlock block)
        {
            for (int i = 0; i < block.Body.Count; i++)
            {
                AstExpression expr = block.Body[i] as AstExpression;
                if (expr != null && expr.Prefixes == null)
                {
                    AstCode op;
                    switch (expr.Code)
                    {
                        case AstCode.Switch:
                        case AstCode.LookupSwitch:
                        case AstCode.Brtrue:
                        case AstCode.Brfalse: 
                            expr.Arguments.Single().ILRanges.AddRange(expr.ILRanges);
                            expr.ILRanges.Clear();
                            continue;
                        case AstCode.__Beq: op = AstCode.Ceq; break;
                        case AstCode.__Bne_Un: op = AstCode.Cne; break;
                        case AstCode.__Bgt: op = AstCode.Cgt; break;
                        case AstCode.__Bgt_Un: op = AstCode.Cgt_Un; break;
                        case AstCode.__Ble: op = AstCode.Cle; break;
                        case AstCode.__Ble_Un: op = AstCode.Cle_Un; break;
                        case AstCode.__Blt: op = AstCode.Clt; break;
                        case AstCode.__Blt_Un: op = AstCode.Clt_Un; break;
                        case AstCode.__Bge: op = AstCode.Cge; break;
                        case AstCode.__Bge_Un: op = AstCode.Cge_Un; break;
                        default:
                            continue;
                    }
                    var newExpr = new AstExpression(expr.SourceLocation, op, null, expr.Arguments);
                    block.Body[i] = new AstExpression(expr.SourceLocation, AstCode.Brtrue, expr.Operand, newExpr);
                    newExpr.ILRanges.AddRange(expr.ILRanges);
                }
            }
        }

        /// <summary>
        /// Group input into a set of blocks that can be later arbitraliby schufled.
        /// The method adds necessary branches to make control flow between blocks
        /// explicit and thus order independent.
        /// </summary>
        private void SplitToBasicBlocks(AstBlock block)
        {
            var basicBlocks = new List<AstNode>();

            var entryLabel = block.Body.FirstOrDefault() as AstLabel ?? new AstLabel(block.SourceLocation, "Block_" + (nextLabelIndex++));
            var basicBlock = new AstBasicBlock(block.SourceLocation);
            basicBlocks.Add(basicBlock);
            basicBlock.Body.Add(entryLabel);
            block.EntryGoto = new AstExpression(block.SourceLocation, AstCode.Br, entryLabel);

            if (block.Body.Count > 0)
            {
                if (block.Body[0] != entryLabel)
                    basicBlock.Body.Add(block.Body[0]);

                for (var i = 1; i < block.Body.Count; i++)
                {
                    var lastNode = block.Body[i - 1];
                    var currNode = block.Body[i];

                    // Start a new basic block if necessary
                    if (currNode is AstLabel ||
                        currNode is AstTryCatchBlock || // Counts as label
                        lastNode.IsConditionalControlFlow() ||
                        lastNode.IsUnconditionalControlFlow())
                    {
                        // Try to reuse the label
                        var label = currNode as AstLabel ?? new AstLabel(currNode.SourceLocation, "Block_" + (nextLabelIndex++));

                        // Terminate the last block
                        if (!lastNode.IsUnconditionalControlFlow())
                        {
                            // Explicit branch from one block to other
                            basicBlock.Body.Add(new AstExpression(lastNode.SourceLocation, AstCode.Br, label));
                        }

                        // Start the new block
                        basicBlock = new AstBasicBlock(currNode.SourceLocation);
                        basicBlocks.Add(basicBlock);
                        basicBlock.Body.Add(label);

                        // Add the node to the basic block
                        if (currNode != label)
                            basicBlock.Body.Add(currNode);
                    }
                    else
                    {
                        basicBlock.Body.Add(currNode);
                    }
                }
            }

            block.Body = basicBlocks;
        }

        static void DuplicateReturnStatements(AstBlock method)
        {
            var nextSibling = new Dictionary<AstLabel, AstNode>();

            // Build navigation data
            foreach (var block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                for (var i = 0; i < block.Body.Count - 1; i++)
                {
                    var curr = block.Body[i] as AstLabel;
                    if (curr != null)
                    {
                        nextSibling[curr] = block.Body[i + 1];
                    }
                }
            }

            // Duplicate returns
            foreach (var block in method.GetSelfAndChildrenRecursive<AstBlock>())
            {
                for (var i = 0; i < block.Body.Count; i++)
                {
                    AstLabel targetLabel;
                    if (block.Body[i].Match(AstCode.Br, out targetLabel) || block.Body[i].Match(AstCode.Leave, out targetLabel))
                    {
                        // Skip extra labels
                        while (nextSibling.ContainsKey(targetLabel) && nextSibling[targetLabel] is AstLabel)
                        {
                            targetLabel = (AstLabel)nextSibling[targetLabel];
                        }

                        // Inline return statement
                        AstNode target;
                        if (nextSibling.TryGetValue(targetLabel, out target))
                        {
                            List<AstExpression> retArgs;
                            if (target.Match(AstCode.Ret, out retArgs))
                            {
                                AstVariable locVar;
                                object constValue;
                                if (retArgs.Count == 0)
                                {
                                    block.Body[i] = new AstExpression(block.Body[i].SourceLocation, AstCode.Ret, null);
                                }
                                else if (retArgs.Single().Match(AstCode.Ldloc, out locVar))
                                {
                                    block.Body[i] = new AstExpression(block.Body[i].SourceLocation, AstCode.Ret, null, new AstExpression(block.Body[i].SourceLocation, AstCode.Ldloc, locVar));
                                }
                                else if (retArgs.Single().Match(AstCode.Ldc_I4, out constValue))
                                {
                                    block.Body[i] = new AstExpression(block.Body[i].SourceLocation, AstCode.Ret, null, new AstExpression(block.Body[i].SourceLocation, AstCode.Ldc_I4, constValue));
                                }
                            }
                        }
                        else
                        {
                            if (method.Body.Count > 0 && method.Body.Last() == targetLabel)
                            {
                                // It exits the main method - so it is same as return;
                                block.Body[i] = new AstExpression(block.Body[i].SourceLocation, AstCode.Ret, null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flattens all nested basic blocks, except the the top level 'node' argument
        /// </summary>
        private static void FlattenBasicBlocks(AstNode node)
        {
            var block = node as AstBlock;
            if (block != null)
            {
                var flatBody = new List<AstNode>();
                foreach (var child in block.GetChildren())
                {
                    FlattenBasicBlocks(child);
                    var childAsBB = child as AstBasicBlock;
                    if (childAsBB != null)
                    {
                        if (!(childAsBB.Body.FirstOrDefault() is AstLabel))
                            throw new Exception("Basic block has to start with a label. \n" + childAsBB);
                        if (childAsBB.Body.LastOrDefault() is AstExpression &&
                           !childAsBB.Body.LastOrDefault().IsUnconditionalControlFlow())
                        {
                            // I got this error when using mapsforge, in createBitmap()
                            //   https://raw.githubusercontent.com/mapsforge/mapsforge/master/mapsforge-map/src/main/java/org/mapsforge/map/rendertheme/XmlUtils.java
                            // Apparently it is related to the finally statement.
                            // I have no idea what it means to disable this check.
                            //throw new Exception("Basic block has to end with unconditional control flow. \n" + childAsBB);
                        }
                            
                        flatBody.AddRange(childAsBB.GetChildren());
                    }
                    else
                    {
                        flatBody.Add(child);
                    }
                }
                block.EntryGoto = null;
                block.Body = flatBody;
            }
            else if (node is AstExpression)
            {
                // Optimization - no need to check expressions
            }
            else if (node != null)
            {
                // Recursively find all ILBlocks
                foreach (var child in node.GetChildren())
                {
                    FlattenBasicBlocks(child);
                }
            }
        }

        /// <summary>
        /// Replace endfinally with jump to the end of the finally block
        /// </summary>
        void RemoveEndFinally(AstBlock method)
        {
            // Go thought the list in reverse so that we do the nested blocks first
            foreach (var tryCatch in method.GetSelfAndChildrenRecursive<AstTryCatchBlock>(tc => tc.FinallyBlock != null).Reverse())
            {
                var label = new AstLabel(tryCatch.FinallyBlock.SourceLocation, "EndFinally_" + nextLabelIndex++);
                tryCatch.FinallyBlock.Body.Add(label);
                foreach (var block in tryCatch.FinallyBlock.GetSelfAndChildrenRecursive<AstBlock>())
                {
                    for (int i = 0; i < block.Body.Count; i++)
                    {
                        if (block.Body[i].Match(AstCode.Endfinally))
                        {
                            block.Body[i] = new AstExpression(block.Body[i].SourceLocation, AstCode.Br, label).WithILRanges(((AstExpression)block.Body[i]).ILRanges);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Merge all variables that have the same original variable into one.
        /// </summary>
        private static void RecombineVariables(AstBlock method)
        {
            // Recombine variables that were split when the ILAst was created
            // This ensures that a single IL variable is a single C# variable (gets assigned only one name)
            // The DeclareVariables transformation might then split up the C# variable again if it is used indendently in two separate scopes.
            var dict = new Dictionary<object, AstVariable>();
            ReplaceVariables(method, v => {
                if (v.OriginalVariable == null)
                    return v;
                AstVariable combinedVariable;
                if (!dict.TryGetValue(v.OriginalVariable, out combinedVariable))
                {
                    dict.Add(v.OriginalVariable, v);
                    combinedVariable = v;
                }
                return combinedVariable;
            });
        }

        /// <summary>
        /// Update the variables used in the given node (and all children) using the given mapping function.
        /// </summary>
        private static void ReplaceVariables(AstNode node, Func<AstVariable, AstVariable> variableMapping)
        {
            var expr = node as AstExpression;
            if (expr != null)
            {
                var v = expr.Operand as AstVariable;
                if (v != null)
                {
                    expr.Operand = variableMapping(v);
                }
                expr.Arguments.ForEach(x => ReplaceVariables(x, variableMapping));
            }
            else
            {
                var catchBlock = node as AstTryCatchBlock.CatchBlock;
                if ((catchBlock != null) && (catchBlock.ExceptionVariable != null))
                {
                    catchBlock.ExceptionVariable = variableMapping(catchBlock.ExceptionVariable);
                }

                foreach (var child in node.GetChildren())
                {
                    ReplaceVariables(child, variableMapping);
                }
            }
        }
    }

    internal static class ILAstOptimizerExtensionMethods
    {
        /// <summary>
        /// Perform one pass of a given optimization on this block.
        /// This block must consist of only basicblocks.
        /// </summary>
        public static bool RunOptimization(this AstBlock block, Func<List<AstNode>, AstBasicBlock, int, bool> optimization)
        {
            bool modified = false;
            List<AstNode> body = block.Body;
            for (int i = body.Count - 1; i >= 0; i--)
            {
                if (i < body.Count && optimization(body, (AstBasicBlock)body[i], i))
                {
                    modified = true;
                }
            }
            return modified;
        }

        public static bool RunOptimization(this AstBlock block, Func<List<AstNode>, AstExpression, int, bool> optimization)
        {
            bool modified = false;
            foreach (AstBasicBlock bb in block.Body)
            {
                for (int i = bb.Body.Count - 1; i >= 0; i--)
                {
                    AstExpression expr = bb.Body.ElementAtOrDefault(i) as AstExpression;
                    if (expr != null && optimization(bb.Body, expr, i))
                    {
                        modified = true;
                    }
                }
            }
            return modified;
        }

        public static bool IsConditionalControlFlow(this AstNode node)
        {
            var expr = node as AstExpression;
            return expr != null && expr.Code.IsConditionalControlFlow();
        }

        public static bool IsUnconditionalControlFlow(this AstNode node)
        {
            var expr = node as AstExpression;
            return expr != null && expr.Code.IsUnconditionalControlFlow();
        }

        /// <summary>
        /// The expression has no effect on the program and can be removed
        /// if its return value is not needed.
        /// </summary>
        public static bool HasNoSideEffects(this AstExpression expr)
        {
            // Remember that if expression can throw an exception, it is a side effect

            switch (expr.Code)
            {
                case AstCode.Ldloc:
                case AstCode.Ldloca:
                case AstCode.Ldstr:
                case AstCode.Ldnull:
                case AstCode.Ldc_I4:
                case AstCode.Ldc_I8:
                case AstCode.Ldc_R4:
                case AstCode.Ldc_R8:
                case AstCode.Ldc_Decimal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStoreToArray(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Stelem_Any:
                case AstCode.Stelem_I:
                case AstCode.Stelem_I1:
                case AstCode.Stelem_I2:
                case AstCode.Stelem_I4:
                case AstCode.Stelem_I8:
                case AstCode.Stelem_R4:
                case AstCode.Stelem_R8:
                case AstCode.Stelem_Ref:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLoadFromArray(this AstCode code)
        {
            switch (code)
            {
                case AstCode.Ldelem_Any:
                case AstCode.Ldelem_I:
                case AstCode.Ldelem_I1:
                case AstCode.Ldelem_I2:
                case AstCode.Ldelem_I4:
                case AstCode.Ldelem_I8:
                case AstCode.Ldelem_U1:
                case AstCode.Ldelem_U2:
                case AstCode.Ldelem_U4:
                case AstCode.Ldelem_R4:
                case AstCode.Ldelem_R8:
                case AstCode.Ldelem_Ref:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Can the expression be used as a statement in C#?
        /// </summary>
        public static bool CanBeExpressionStatement(this AstExpression expr)
        {
            switch (expr.Code)
            {
                case AstCode.Call:
                case AstCode.Callvirt:
                case AstCode.CallIntf:
                case AstCode.CallSpecial:
                    // property getters can't be expression statements, but all other method calls can be
                    var mr = (XMethodReference)expr.Operand;
                    return !mr.Name.StartsWith("get_", StringComparison.Ordinal);
                case AstCode.Newobj:
                case AstCode.Newarr:
                case AstCode.MultiNewarr:
                case AstCode.Stloc:
                case AstCode.Stobj:
                case AstCode.Stsfld:
                case AstCode.Stfld:
                case AstCode.Stind_Ref:
                case AstCode.Stelem_Any:
                case AstCode.Stelem_I:
                case AstCode.Stelem_I1:
                case AstCode.Stelem_I2:
                case AstCode.Stelem_I4:
                case AstCode.Stelem_I8:
                case AstCode.Stelem_R4:
                case AstCode.Stelem_R8:
                case AstCode.Stelem_Ref:
                    return true;
                default:
                    return false;
            }
        }

        public static AstExpression WithILRanges(this AstExpression expr, IEnumerable<InstructionRange> ilranges)
        {
            expr.ILRanges.AddRange(ilranges);
            return expr;
        }

        public static void RemoveTail(this List<AstNode> body, params AstCode[] codes)
        {
            for (int i = 0; i < codes.Length; i++)
            {
                if (((AstExpression)body[body.Count - codes.Length + i]).Code != codes[i])
                    throw new Exception("Tailing code does not match expected.");
            }
            body.RemoveRange(body.Count - codes.Length, codes.Length);
        }

        public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key)
        {
            V ret;
            dict.TryGetValue(key, out ret);
            return ret;
        }

        public static void RemoveOrThrow<T>(this ICollection<T> collection, T item)
        {
            if (!collection.Remove(item))
                throw new Exception("The item was not found in the collection");
        }

        public static void RemoveOrThrow<K, V>(this Dictionary<K, V> collection, K key)
        {
            if (!collection.Remove(key))
                throw new Exception("The key was not found in the dictionary");
        }
    }
}
