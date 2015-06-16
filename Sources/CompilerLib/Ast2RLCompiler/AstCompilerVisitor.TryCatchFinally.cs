using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;
using Catch = Dot42.CompilerLib.RL.Catch;
using ExceptionHandler = Dot42.CompilerLib.RL.ExceptionHandler;
using Instruction = Dot42.CompilerLib.RL.Instruction;
using Register = Dot42.CompilerLib.RL.Register;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    partial class AstCompilerVisitor
    {
        /// <summary>
        /// Create try/catch/finally/fault block
        /// </summary>
        public override RLRange Visit(AstTryCatchBlock node, AstNode parent)
        {
            var handler = new ExceptionHandler();

            //if (node.FaultBlock != null)
            //{
            //    Debugger.Break();
            //}

            // Setup instruction before/after my node.
            var first = new Instruction() { SequencePoint = node.SourceLocation};
            var last = new Instruction(RCode.Nop);

            FinallyBlockState finState = null;
            FinallyBlockState outerFinState = tryCatchStack.FirstOrDefault(f => f.HasFinally);

            if (tryCatchStack.Count == 0)
                finallyState.FinallyStacks.Add(new List<FinallyBlockState>());

            if (node.FinallyBlock != null)
            {
                // store finaly state
                finState = new FinallyBlockState(outerFinState, tryCatchStack.Count, this, first, last);

                finState.FinallyExceptionRegister = frame.AllocateTemp(new ClassReference("java.lang.Throwable")).Register;
                // clear the variable to make sure it isn't stale from a previous loop.
                // make sure this is outside the try block for edge case exceptions.
                this.Add(node.SourceLocation, RCode.Const, 0, finState.FinallyExceptionRegister);

                tryCatchStack.Push(finState);
                finallyState.FinallyStacks.Last().Add(finState);
            }
            else
            {
                finState = new FinallyBlockState(outerFinState, tryCatchStack.Count);
                tryCatchStack.Push(finState);
            }

            instructions.Add(first);
            // Emit try block
            handler.TryStart = first;
            node.TryBlock.AcceptOrDefault(this, node);
            handler.TryEnd = TryCatchGotoEnd(finState, last);

            var catchesStart = this.Add(AstNode.NoSource, RCode.Nop);

            // Emit "normal" catch blocks
            foreach (var catchBlock in node.CatchBlocks.Where(x => !x.IsCatchAll()))
            {
                var c = new Catch { Type = catchBlock.ExceptionType.GetReference(targetPackage) };
                handler.Catches.Add(c);
                var catchStart = this.Add(catchBlock.SourceLocation, RCode.Nop);
                catchBlock.Accept(this, node);
                c.Instruction = catchStart;

                TryCatchGotoEnd(finState, last);
            }

            // Emit "catch all" (if any)
            var catchAllBlock = node.CatchBlocks.SingleOrDefault(x => x.IsCatchAll());
            if (catchAllBlock != null)
            {
                var catchStart = this.Add(catchAllBlock.SourceLocation, RCode.Nop);
                catchAllBlock.Accept(this, node);
                handler.CatchAll = catchStart;

                TryCatchGotoEnd(finState, last);
            }

            var catchesEnd = this.Add(AstNode.NoSource, RCode.Nop);

            // clear try/catch/finally stack: we don't want to cover ourselves!
            tryCatchStack.Pop();

            // Emit finally code
            if (node.FinallyBlock != null)
            {
                // preparation.
                var finallyStart = this.Add(node.FinallyBlock.SourceLocation, RCode.Move_exception,
                                            finState.FinallyExceptionRegister);
                instructions.Add(finState.NonException);

                // the original handler
                node.FinallyBlock.Accept(this, node);

                // prepare the routing
                this.Add(AstNode.NoSource, RCode.If_eqz, finState.AfterExceptionCheck, finState.FinallyExceptionRegister);
                this.Add(AstNode.NoSource, RCode.Throw, finState.FinallyExceptionRegister);
                instructions.Add(finState.AfterExceptionCheck);

                // Set up exception handlers.
                if (catchAllBlock == null)
                {
                    // we need to cover the try block.
                    handler.CatchAll = finallyStart;
                }

                if (node.CatchBlocks.Any())
                {
                    // we need to cover the catch blocks
                    var finallyHandler = new ExceptionHandler
                    {
                        TryStart = catchesStart,
                        TryEnd = catchesEnd,
                        CatchAll = finallyStart
                    };
                    body.Exceptions.Add(finallyHandler);
                }
            }

            // Add end
            instructions.Add(last);

            // Record catch/catch-all handler
            if ((handler.CatchAll != null) || handler.Catches.Any())
            {
                body.Exceptions.Add(handler);
            }

            return new RLRange(first, last, null);
        }

        private Instruction TryCatchGotoEnd(FinallyBlockState state, Instruction last)
        {
            if (state.HasFinally)
            {
                var rlrange = state.BranchToFinally_FallOut(AstNode.NoSource, last, new List<RLRange>());
                return rlrange.Last;
            }

            // goto end 
            return this.Add(AstNode.NoSource, RCode.Goto, last);    
        }

        private void FixFinallyRouting()
        {
            Debug.Assert(tryCatchStack.Count == 0);

            foreach (var finallyStack in finallyState.FinallyStacks)
            {
                FixFinallyStack(finallyStack);
            }
        }

        private void FixFinallyStack(List<FinallyBlockState> finallyStack)
        {
            // Find all identical targets in this stack.
            // To minimize assigments, order by number of targets. To simplify the routing 
            // code when possible, prefer IsFallOut over IsLeave over IsReturn
            var targets = TryCatchGroupTargets(finallyStack.SelectMany(f => f.Targets));

            if (targets.Count == 0)
                return;

            // assign target group ids. zero is the default, and does not need to be set.
            int id = -1;
            foreach (var targetGroup in targets)
            {
                ++id;
                foreach (var target in targetGroup)
                {
                    if (id == 0) target.SetTarget.ConvertToNop();
                    else         target.SetTarget.Operand = id;
                }
            }

            // now work from the innermost to the outermost.
            var finallyBlocks = finallyStack.Where(f=>f.HasFinally)
                                            .OrderByDescending(p=>p.Depth)
                                            .ToList();
            foreach(var finallyBlock in finallyBlocks)
            {
                targets = TryCatchGroupTargets(finallyBlock.Targets);

                // reset 'leave' instruction that do not leave this block.
                foreach (var targetGroup in targets.Where(t => t.Key.IsLeave).ToList())
                {
                    bool staysInBlock = targetGroup.Key.Destination.Index >= finallyBlock.FirstInstruction.Index 
                                    &&  targetGroup.Key.Destination.Index <= finallyBlock.LastInstruction.Index;

                    if(staysInBlock)
                    {
                        foreach (var source in targetGroup)
                        {
                            source.GotoFinally.Operand = source.Destination;
                            source.SetTarget.ConvertToNop();
                        }
                        targets.Remove(targetGroup);
                    }
                }

                if (targets.Count == 0)
                    continue;

                bool needTargetRegister = targets.Count > 1;
                TryCatchSetTargetRegister(finallyBlock, needTargetRegister);

                var outerFinallyBlock = finallyBlock.OuterFinallyBlock;
                int insIdx = finallyBlock.AfterExceptionCheck.Index + 1;

                if (targets.Count == 1)
                {
                    // primitive case
                    var target = targets[0];
                    TryCatchEmitTargetInstruction(target.Key, ref insIdx, outerFinallyBlock);
                }
                else if (targets.Count == 2)
                {
                    // use a single comparison
                    var target = targets[0].Key;
                    id = target.SetTarget.Code == RCode.Nop ? 0 : (int)target.SetTarget.Operand;

                    Instruction compare;
                    if (id == 0)
                    {
                        compare = new Instruction(RCode.If_nez, finallyBlock.TargetRegister);
                    }
                    else
                    {
                        var comparisonRegister = frame.AllocateTemp(PrimitiveType.Int);
                        var iConst = new Instruction(RCode.Const, id, comparisonRegister.Registers.ToArray());
                        instructions.Insert(insIdx++, iConst);
                        compare = new Instruction(RCode.If_ne, finallyBlock.TargetRegister, comparisonRegister);
                    }
                    instructions.Insert(insIdx++, compare);
                    TryCatchEmitTargetInstruction(target, ref insIdx, outerFinallyBlock);

                    target = targets[1].Key;
                    var def = TryCatchEmitTargetInstruction(target, ref insIdx, outerFinallyBlock);
                    compare.Operand = def;
                }
                else
                {
                    // use a sparse-switch
                    List<Tuple<int,Instruction>> sparseSwitchData = new List<Tuple<int, Instruction>>();
                    var ps = new Instruction(RCode.Sparse_switch, finallyBlock.TargetRegister);
                    instructions.Insert(insIdx++, ps);

                    // emit default.
                    TryCatchEmitTargetInstruction(targets.Last().Key, ref insIdx, outerFinallyBlock);

                    foreach (var targetGrouping in targets.Take(targets.Count - 1))
                    {
                        var target = targetGrouping.Key;
                        id = target.SetTarget.Code == RCode.Nop ? 0 : (int)target.SetTarget.Operand;
                        
                        var first = TryCatchEmitTargetInstruction(target, ref insIdx, outerFinallyBlock);
                        sparseSwitchData.Add(Tuple.Create(id, first));
                    }
                    ps.Operand = sparseSwitchData.ToArray();
                }
            }
        }

        /// <summary>
        /// return the first emitted instruction
        /// </summary>
        private Instruction TryCatchEmitTargetInstruction(FinallyTarget target, ref int insIdx, FinallyBlockState outerFinallyBlock)
        {
            bool emitReturn = outerFinallyBlock == null && target.IsReturn;

            bool chainToOuterBlock = outerFinallyBlock != null && target.IsReturn;

            if (target.IsLeave && outerFinallyBlock != null)
            {
                // check if the leave leaves the outer finally block as well.
                if (target.Destination.Index < outerFinallyBlock.FirstInstruction.Index
                 || target.Destination.Index > outerFinallyBlock.LastInstruction.Index)
                {
                    chainToOuterBlock = true;
                }
            }

            if (emitReturn)
            {
                Instruction ret;
                if (currentMethod.ReturnsVoid)
                {
                    ret = new Instruction(RCode.Return_void);
                }
                else
                {
                    var retCode = currentMethod.ReturnsDexWide
                        ? RCode.Return_wide
                        : currentMethod.ReturnsDexValue
                            ? RCode.Return
                            : RCode.Return_object;
                    ret = new Instruction(retCode, finallyState.ReturnValueRegister);
                }
                instructions.Insert(insIdx++, ret);
                return ret;
            }
            else if (chainToOuterBlock)
            {
                Debug.Assert(outerFinallyBlock != null);
                int id = target.SetTarget.Code == RCode.Nop ? 0 : (int)target.SetTarget.Operand;

                RLRange range;
                if (target.IsReturn)
                    range = outerFinallyBlock.BranchToFinally_Ret(ref insIdx);
                else // IsLeave
                    range = outerFinallyBlock.BranchToFinally_Leave(target.Destination, ref insIdx);

                // This is a little bit hackish. We need to set the operand in the setTarget instruction.
                if (id == 0) range.First.ConvertToNop();
                else range.First.Operand = id;

                return range.First;
            }
            else
            {
                // goto
                var insGoto = new Instruction(RCode.Goto, target.Destination);
                instructions.Insert(insIdx++, insGoto);
                return insGoto;
            }
        }

        private void TryCatchSetTargetRegister(FinallyBlockState finallyBlock, bool needTargetRegister)
        {
            var localTargets = finallyBlock.Targets.Where(t => t.State == finallyBlock);

            if (needTargetRegister)
            {
                finallyBlock.TargetRegister = Frame.AllocateTemp(PrimitiveType.Int);
                var setToZero = new Instruction(RCode.Const, 0, new []{ finallyBlock.TargetRegister});
                instructions.Insert(finallyBlock.FirstInstruction.Index, setToZero);
                
                foreach (var t in localTargets)
                {
                    if (t.SetTarget.Code != RCode.Nop)
                        t.SetTarget.Registers.Add(finallyBlock.TargetRegister);
                }
            }
            else
            {
                foreach (var t in localTargets)
                    t.SetTarget.ConvertToNop();
            }
        }

        private List<IGrouping<FinallyTarget, FinallyTarget>> TryCatchGroupTargets(IEnumerable<FinallyTarget> targets)
        {
            return targets.GroupBy(t=>t)
                          .OrderByDescending(t => t.Count()*3 + (t.Key.IsFallOut?2:t.Key.IsLeave?1:0))
                          .ToList();
        }

        /// <summary>
        /// Create code for given catch block
        /// </summary>
        public override RLRange Visit(AstTryCatchBlock.CatchBlock node, AstNode parent)
        {
            // Allocate exception register
            var r = frame.AllocateTemp(node.ExceptionType.GetReference(targetPackage));
            var first = this.Add(node.SourceLocation, RCode.Move_exception, r);
            var last = first;

            currentExceptionRegister.Push(r);

            // Store exception in exception variable (if any)
            if (node.ExceptionVariable != null)
            {
                var rVar = frame.GetArgument(node.ExceptionVariable);
                this.Add(node.SourceLocation, RCode.Move_object, rVar, r);
            }

            // Generate code for actual catch block.
            var result = Visit((AstBlock)node, parent);
            if (result != null)
            {
                last = result.Last;
            }

            currentExceptionRegister.Pop();

            // Combine result
            return new RLRange(first, last, null);
        }

        private class FinallyBlockState
        {
            private readonly AstCompilerVisitor _compiler;

            /// <summary>
            /// Denotes the register holding the final target instruction 
            /// when executing finally blocks. We have one register per block
            /// (as opposed to per method) to keep the number of assigments small
            /// while not running into trouble with exceptions in finally blocks.
            /// </summary>
            public Register TargetRegister;

            /// <summary>
            /// denotes the register holding an exception in a finally block.
            /// </summary>
            public Register FinallyExceptionRegister;

            /// <summary>
            /// jump here to invoke the finally handler.
            /// </summary>
            public readonly Instruction NonException;
            public readonly Instruction AfterExceptionCheck;

            public readonly Instruction FirstInstruction;
            public readonly Instruction LastInstruction;

            public readonly FinallyBlockState OuterFinallyBlock;
            
            internal readonly List<FinallyTarget> Targets = new List<FinallyTarget>();

            public int Depth { get; private set; }
            public bool HasFinally { get { return NonException != null; } }

            public FinallyBlockState(FinallyBlockState outerBlock, int depth)
            {
                Depth = depth;
                OuterFinallyBlock = outerBlock;
            }

            public FinallyBlockState(FinallyBlockState outerBlock, int depth, AstCompilerVisitor compiler, Instruction first, Instruction last)
            {
                _compiler = compiler;
                NonException = new Instruction();
                AfterExceptionCheck = new Instruction();
                FirstInstruction = first;
                LastInstruction = last;
                Depth = depth;
                OuterFinallyBlock = outerBlock;
            }

            private RLRange BranchToFinally(FinallyTarget target, IEnumerable<RLRange> prefix, ISourceLocation seqp, ref int insIdx)
            {
                Debug.Assert(this.HasFinally);

                Targets.Add(target);

                var setTarget = new Instruction(RCode.Const) { SequencePoint = seqp }; // operand and register are set later
                _compiler.instructions.Insert(insIdx++, setTarget); 
                var branch = new Instruction(RCode.Goto, NonException) {SequencePoint = seqp};
                _compiler.instructions.Insert(insIdx++, branch);

                target.SetTarget = setTarget;
                target.GotoFinally = branch;
                target.State = this;

                return new RLRange(prefix, setTarget, branch, null);
            }

            public RLRange BranchToFinally_Leave(ISourceLocation sourceLocation, AstLabel labelTarget, List<RLRange> args)
            {
                int insIdx = _compiler.instructions.Count;
                var target = new FinallyTarget {IsLeave = true };
                _compiler.labelManager.AddResolveAction(labelTarget, ins=> target.Destination = ins);
                return BranchToFinally(target, args, sourceLocation, ref insIdx);
            }

            public RLRange BranchToFinally_FallOut(ISourceLocation sourceLocation, Instruction insTarget, List<RLRange> args)
            {
                int insIdx = _compiler.instructions.Count;
                var target = new FinallyTarget {Destination = insTarget, IsFallOut = true};
                return BranchToFinally(target, args, sourceLocation, ref insIdx);
            }

            public RLRange BranchToFinally_Ret(ISourceLocation sourceLocation, List<RLRange> args)
            {
                int insIdx = _compiler.instructions.Count;
                var target = new FinallyTarget {IsReturn = true};
                return BranchToFinally(target, args, sourceLocation, ref insIdx);
            }

            public RLRange BranchToFinally_Ret(ref int insIdx)
            {
                var target = new FinallyTarget { IsReturn = true };
                return BranchToFinally(target, null, AstNode.NoSource, ref insIdx);
            }

            public RLRange BranchToFinally_Leave(Instruction insTarget, ref int insIdx)
            {
                var target = new FinallyTarget { IsLeave = true, Destination = insTarget};
                return BranchToFinally(target, null, AstNode.NoSource, ref insIdx);
            }
        }

        private class FinallyTarget
        {
            public bool IsReturn;
            public bool IsFallOut;
            public bool IsLeave; 
            
            public Instruction SetTarget;
            public Instruction GotoFinally;
            public Instruction Destination;

            public FinallyBlockState State;

            protected bool Equals(FinallyTarget other)
            {
                return (IsReturn && other.IsReturn)
                    || (IsFallOut && other.IsFallOut)
                    || Equals(Destination, other.Destination);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    if (IsReturn) return 397;
                    if (IsFallOut) return 33;
                    return Destination.GetHashCode()*397;
                }
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((FinallyTarget)obj);
            }
        }

        private class MethodFinallyState
        {
            /// <summary>
            /// allocate to hold the return value, if any, if we are to leave
            /// the method after all finally blocks have executed.
            /// </summary>
            public RegisterSpec ReturnValueRegister;

            public readonly List<List<FinallyBlockState>> FinallyStacks = new List<List<FinallyBlockState>>();
        }

    }
}
