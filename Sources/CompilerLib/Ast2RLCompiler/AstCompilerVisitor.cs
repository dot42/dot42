using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    /// <summary>
    /// AstNode visitor that generates RL instructions.
    /// </summary>
    internal sealed partial class AstCompilerVisitor : AstNodeVisitor<RLRange, AstNode>, IRLBuilder
    {
        private readonly AssemblyCompiler compiler;
        private readonly MethodSource currentMethod;
        private readonly Dot42.DexLib.MethodDefinition currentDexMethod;
        private readonly DexTargetPackage targetPackage;
        private readonly MethodBody body;
        private readonly AstInvocationFrame frame;
        private readonly InstructionList instructions;
        private readonly LabelManager labelManager = new LabelManager();
        private Register currentExceptionRegister; // Holds the exception in a catch block

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AstCompilerVisitor(AssemblyCompiler compiler, MethodSource source, DexTargetPackage targetPackage, Dot42.DexLib.MethodDefinition method, MethodBody body)
        {
            this.compiler = compiler;
            currentMethod = source;
            this.targetPackage = targetPackage;
            currentDexMethod = method;
            this.body = body;
            frame = new AstInvocationFrame(targetPackage, method, source, body);
            instructions = body.Instructions;

            // Base class class ctor for structs
            if (source.IsDotNet && (source.Name == ".ctor") && (source.ILMethod.DeclaringType.IsValueType))
            {
                var ilMethod = source.ILMethod;
                if (!HasBaseOrThisClassCtorCall(ilMethod))
                {
                    // Add a call to the base class ctor now
                    var seqp = SequencePointWrapper.Wrap(ilMethod.Body.Instructions.Select(x => x.SequencePoint).FirstOrDefault());
                    var baseCtor = new MethodReference(method.Owner.SuperClass, "<init>", new Prototype(PrimitiveType.Void));
                    this.Add(seqp, RCode.Invoke_direct, baseCtor, frame.ThisArgument);
                }
            }

            // Store any genericInstance argument
            /*if (source.Method.NeedsGenericInstanceTypeParameter && (source.Name == ".ctor"))
            {
                var owner = method.Owner;
                var ilMethod = source.ILMethod;
                var giField = owner.GenericInstanceField;
                if (giField == null)
                {
                    throw new CompilerException(string.Format("Expected GenericInstance field in {0}", ilMethod.FullName));
                }
                var seqp = SequencePointWrapper.Wrap(ilMethod.Body.Instructions.Select(x => x.SequencePoint).FirstOrDefault());
                this.Add(seqp, RCode.Iput_object, giField, frame.GenericInstanceTypeArgument, frame.ThisArgument);
            }*/
        }

        /// <summary>
        /// Gets the used invocation frame.
        /// </summary>
        internal InvocationFrame Frame
        {
            get { return frame; }
        }

        /// <summary>
        /// The largest number of registers used in a call to another method.
        /// </summary>
        internal int MaxOutgoingRegisters { get; private set; }

        /// <summary>
        /// Expand the visit of a block to each node.
        /// </summary>
        private RLRange Visit(IEnumerable<AstNode> nodes, AstNode parent)
        {
            var ranges = nodes.Select(x => x.Accept(this, parent)).ToList();
            return ranges.Combine();
        }

        /// <summary>
        /// Expand the visit of a block to each node.
        /// </summary>
        public override RLRange Visit(AstBlock node, AstNode parent)
        {
            return Visit(node.GetChildren(), node);
        }

        /// <summary>
        /// Expand the visit of a block to each node.
        /// </summary>
        public override RLRange Visit(AstBasicBlock node, AstNode parent)
        {
            return Visit(node.GetChildren(), node);
        }

        /// <summary>
        /// Record the given label
        /// </summary>
        public override RLRange Visit(AstLabel node, AstNode parent)
        {
            var nop = this.Add(node.SourceLocation, RCode.Nop);
            labelManager.SetTarget(node, nop);
            return new RLRange(nop, null);
        }

        /// <summary>
        /// Generate code for the given expression.
        /// </summary>
        public override RLRange Visit(AstExpression node, AstNode parent)
        {
            // Have we evaluated this before?
            if (node.Result != null) return (RLRange) node.Result;

            // Generate dex expressions for each argument
            List<RLRange> args ;
            switch (node.Code)
            {
                case AstCode.Conditional:
                    args = node.Arguments.Take(1).Select(x => x.Accept(this, node)).ToList();
                    break;
                default:
                    args = node.Arguments.Select(x => x.Accept(this, node)).ToList();
                    break;
            }

            // Generate code for actual expression itself
            var result = VisitExpression(node, args, parent);
            node.Result = result;

            // Return result
            return result;
        }

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
            currentExceptionRegister = r;

            // Store exception in exception variable (if any)
            if (node.ExceptionVariable != null)
            {
                var rVar = frame.GetArgument(node.ExceptionVariable);
                this.Add(node.SourceLocation, RCode.Move_object, rVar, r);
            }

            // Generate code for actual catch block.
            var result = Visit((AstBlock) node, parent);
            if (result != null)
            {
                last = result.Last;
            }

            // Combine result
            currentExceptionRegister = null;
            return new RLRange(first, last, null);
        }

        /// <summary>
        /// Create and add an instruction.
        /// </summary>
        public Instruction Add(ISourceLocation sequencePoint, RCode opcode, object operand, IEnumerable<Register> registers)
        {
            return instructions.Add(sequencePoint, opcode, operand, registers);
        }

        /// <summary>
        /// Does the given method have a call to a base class ctor?
        /// </summary>
        private static bool HasBaseOrThisClassCtorCall(MethodDefinition method)
        {
            var resolver = new GenericsResolver(method.DeclaringType);
            foreach (var ins in method.Body.Instructions.Where(x => x.OpCode.Code == Mono.Cecil.Cil.Code.Call))
            {
                var target = ins.Operand as Mono.Cecil.MethodReference;
                if ((target != null) && (target.Name == ".ctor") && 
                    ((target.DeclaringType.AreSame(method.DeclaringType, resolver.Resolve)) ||
                    (target.DeclaringType.AreSame(method.DeclaringType.BaseType, resolver.Resolve))))
                {
                    return true;
                }
            }
            return false;
        }
    }
}