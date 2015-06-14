using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

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

            // Setup instruction after my node.
            var last = new Instruction(RCode.Nop);

            // Emit try block
            handler.TryStart = this.Add(node.SourceLocation, RCode.Nop);
            node.TryBlock.AcceptOrDefault(this, node);
            handler.TryEnd = this.Add(node.SourceLocation, RCode.Nop);

            // Emit finally code
            if (node.FinallyBlock != null)
            {
                using (labelManager.CreateContext())
                {
                    node.FinallyBlock.Accept(this, node);
                    node.FinallyBlock.CleanResults();
                }
            }

            // Jump to end
            this.Add(node.SourceLocation, RCode.Goto, last);

            // Replace returns in the try block
            InsertFinallyBeforeReturns(handler.TryStart, handler.TryEnd, node);

            // Prepare for finally handler
            var finallyStart = (node.FinallyBlock != null) ? new Instruction(RCode.Move_exception) : null;

            // Emit "normal" catch blocks
            foreach (var catchBlock in node.CatchBlocks.Where(x => !x.IsCatchAll()))
            {
                var c = new Catch { Type = catchBlock.ExceptionType.GetReference(targetPackage) };
                handler.Catches.Add(c);
                var catchStart = this.Add(catchBlock.SourceLocation, RCode.Nop);
                catchBlock.Accept(this, node);
                var catchEnd = this.Add(catchBlock.SourceLocation, RCode.Nop);
                c.Instruction = catchStart;

                // Add finally block
                if (node.FinallyBlock != null)
                {
                    // Emit finally code
                    using (labelManager.CreateContext())
                    {
                        node.FinallyBlock.Accept(this, node);
                        node.FinallyBlock.CleanResults();
                    }

                    // Write catch block in finally handler
                    var catchFinallyHandler = new ExceptionHandler { TryStart = catchStart, TryEnd = catchEnd, CatchAll = finallyStart };
                    body.Exceptions.Add(catchFinallyHandler);
                }

                // Jump to end
                this.Add(node.SourceLocation, RCode.Goto, last);

                // Replace returns in the catch block
                InsertFinallyBeforeReturns(catchStart, catchEnd, node);
            }

            // Emit "catch all" (if any)
            var catchAllBlock = node.CatchBlocks.SingleOrDefault(x => x.IsCatchAll());
            if (catchAllBlock != null)
            {
                var catchStart = this.Add(catchAllBlock.SourceLocation, RCode.Nop);
                catchAllBlock.Accept(this, node);
                var catchEnd = this.Add(catchAllBlock.SourceLocation, RCode.Nop);
                handler.CatchAll = catchStart;

                // Add finally block
                if (node.FinallyBlock != null)
                {
                    // Emit finally code
                    using (labelManager.CreateContext())
                    {
                        node.FinallyBlock.Accept(this, node);
                        node.FinallyBlock.CleanResults();
                    }

                    // Write catch block in finally handler
                    var catchFinallyHandler = new ExceptionHandler { TryStart = catchStart, TryEnd = catchEnd, CatchAll = finallyStart };
                    body.Exceptions.Add(catchFinallyHandler);
                }

                // Jump to end
                this.Add(node.SourceLocation, RCode.Goto, last);

                // Replace returns in the catch block
                InsertFinallyBeforeReturns(catchStart, catchEnd, node);
            }

            // Emit finally block (if any)
            if (node.FinallyBlock != null)
            {
                // Create finally handler
                var finallyHandler = new ExceptionHandler { TryStart = handler.TryStart, TryEnd = handler.TryEnd/*body.Instructions.Last()*/, CatchAll = finallyStart };
                body.Exceptions.Add(finallyHandler);

                // Update move_exception instructions
                var r = frame.AllocateTemp(new ClassReference("java.lang.Throwable"));
                finallyStart.Registers.Add(r);
                instructions.Add(finallyStart);

                // Emit finally code
                using (labelManager.CreateContext())
                {
                    node.FinallyBlock.Accept(this, node);
                    node.FinallyBlock.CleanResults();
                }

                // Re-throw exception
                this.Add(node.SourceLocation, RCode.Throw, r);
            }

            // Add end
            instructions.Add(last);

            // Record catch/catch-all handler
            if ((handler.CatchAll != null) || handler.Catches.Any())
            {
                body.Exceptions.Add(handler);
            }

            return new RLRange(handler.TryStart, last, null);
        }

        /// <summary>
        /// Insert the finally block of the given try/catch block before every "exit" opcode in the given range.
        /// </summary>
        private void InsertFinallyBeforeReturns(Instruction first, Instruction last, AstTryCatchBlock tryCatchBlock)
        {
            if (tryCatchBlock.FinallyBlock == null)
                return;

            // Find all return instructions
            var firstIndex = first.Index;
            var lastIndex = last.Index;
            var exitInstructions = new List<Instruction>();
            for (var index = firstIndex; index <= lastIndex; index++)
            {
                var ins = instructions[index];
                if (ins.Code.IsReturn() /*|| (ins.Code == RCode.Leave)*/)
                {
                    exitInstructions.Add(ins);
                }
            }
            if (exitInstructions.Count == 0)
                return;

            // Insert finally block before each return instruction
            foreach (var ins in exitInstructions)
            {
                // Clone the exit instruction
                var clone = new Instruction(ins);

                // Emit finally block
                var finallyStart = this.Add(tryCatchBlock.FinallyBlock.SourceLocation, RCode.Nop);
                using (labelManager.CreateContext())
                {
                    tryCatchBlock.FinallyBlock.Accept(this, tryCatchBlock);
                    tryCatchBlock.FinallyBlock.CleanResults();
                }

                // Convert instruction -> jump to finally block
                ins.Code = RCode.Goto;
                ins.Operand = finallyStart;
                ins.Registers.Clear();

                // Append exit instruction
                instructions.Add(clone);
            }
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

            var previousExceptionRegister = currentExceptionRegister;
            currentExceptionRegister = r;

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

            // Combine result
            currentExceptionRegister = previousExceptionRegister;
            return new RLRange(first, last, null);
        }
    }
}
