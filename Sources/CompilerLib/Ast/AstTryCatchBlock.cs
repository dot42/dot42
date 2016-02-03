using System.Collections.Generic;
using Dot42.CompilerLib.XModel;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Try/catch/finally block
    /// </summary>
    public sealed class AstTryCatchBlock : AstNode
    {
        private readonly List<CatchBlock> catchBlocks = new List<CatchBlock>();
        public AstBlock TryBlock;
        public AstBlock FinallyBlock;
        public AstBlock FaultBlock;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstTryCatchBlock(ISourceLocation sourceLocation)
            : base(sourceLocation)
        {
        }

        /// <summary>
        /// All catch blocks
        /// </summary>
        public List<CatchBlock> CatchBlocks { get { return catchBlocks; } }

        /// <summary>
        /// Gets all direct child nodes.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            if (TryBlock != null)
                yield return TryBlock;
            foreach (var catchBlock in CatchBlocks)
            {
                yield return catchBlock;
            }
            if (FaultBlock != null)
                yield return FaultBlock;
            if (FinallyBlock != null)
                yield return FinallyBlock;
        }

        /// <summary>
        /// Write human readable output.
        /// </summary>
        public override void WriteTo(ITextOutput output, FormattingOptions format)
        {
            output.WriteLine(".try {");
            output.Indent();
            TryBlock.WriteTo(output, format);
            output.Unindent();
            output.WriteLine("}");
            foreach (var block in CatchBlocks)
            {
                block.WriteTo(output, format);
            }
            if (FaultBlock != null)
            {
                output.WriteLine("fault {");
                output.Indent();
                FaultBlock.WriteTo(output, format);
                output.Unindent();
                output.WriteLine("}");
            }
            if (FinallyBlock != null)
            {
                output.WriteLine("finally {");
                output.Indent();
                FinallyBlock.WriteTo(output, format);
                output.Unindent();
                output.WriteLine("}");
            }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(AstNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Catch block 
        /// </summary>
        public sealed class CatchBlock : AstBlock
        {
            private readonly AstTryCatchBlock parent;
            public XTypeReference ExceptionType;
            public AstVariable ExceptionVariable;

            /// <summary>
            /// Default ctor
            /// </summary>
            public CatchBlock(ISourceLocation sourceLocation, AstTryCatchBlock parent)
                : base(sourceLocation)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Gets the containing try block.
            /// </summary>
            public AstTryCatchBlock Parent { get { return parent; } }

            /// <summary>
            /// Write human readable output.
            /// </summary>
            public override void WriteTo(ITextOutput output, FormattingOptions format)
            {
                output.Write("catch ");
                output.WriteReference(ExceptionType.FullName, ExceptionType);
                if (ExceptionVariable != null)
                {
                    output.Write(' ');
                    output.Write(ExceptionVariable.Name);
                }
                output.WriteLine(" {");
                output.Indent();
                base.WriteTo(output, format);
                output.Unindent();
                output.WriteLine("}");
            }

            /// <summary>
            /// Accept a visit by the given visitor.
            /// </summary>
            public override TReturn Accept<TReturn, TData>(AstNodeVisitor<TReturn, TData> visitor, TData data)
            {
                return visitor.Visit(this, data);
            }
        }
    }
}