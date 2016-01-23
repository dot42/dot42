using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Mono.Cecil.Cil;
using Instruction = Dot42.CompilerLib.RL.Instruction;
using MethodBody = Dot42.CompilerLib.RL.MethodBody;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    /// <summary>
    /// AstNode visitor that generates RL instructions.
    /// </summary>
    internal sealed partial class AstCompilerVisitor : AstNodeVisitor<RLRange, AstNode>, IRLBuilder
    {
        private readonly AssemblyCompiler compiler;
        private readonly MethodSource currentMethod;
        private readonly MethodDefinition currentDexMethod;
        private readonly DexTargetPackage targetPackage;
        private readonly MethodBody body;
        private readonly AstInvocationFrame frame;
        private readonly InstructionList instructions;
        private readonly LabelManager labelManager = new LabelManager();
        private readonly Stack<Register> currentExceptionRegister = new Stack<Register>();        // Holds the exception in a catch block
        private readonly MonitorManager monitorManager = new MonitorManager();
        private readonly MethodFinallyState finallyState = new MethodFinallyState();
        private readonly Stack<FinallyBlockState> tryCatchStack = new Stack<FinallyBlockState>(); // holds the current finally target.

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AstCompilerVisitor(AssemblyCompiler compiler, MethodSource source, DexTargetPackage targetPackage, MethodDefinition method, MethodBody body)
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
                case AstCode.NullCoalescing:
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
        /// Create and add an instruction.
        /// </summary>
        public Instruction Add(ISourceLocation sequencePoint, RCode opcode, object operand, IEnumerable<Register> registers)
        {
            return instructions.Add(sequencePoint, opcode, operand, registers);
        }

        /// <summary>
        /// Does the given method have a call to a base class ctor?
        /// </summary>
        private static bool HasBaseOrThisClassCtorCall(Mono.Cecil.MethodDefinition method)
        {
            var resolver = new GenericsResolver(method.DeclaringType);
            foreach (var ins in method.Body.Instructions.Where(x => x.OpCode.Code == Code.Call))
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

        public void Complete()
        {
            FixFinallyRouting();
        }
    }
}