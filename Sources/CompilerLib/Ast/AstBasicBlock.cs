using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Block with single entry and single exit point.
    /// </summary>
    public sealed class AstBasicBlock : AstNode
    {
        /// <remarks> Body has to start with a label and end with unconditional control flow </remarks>
        public readonly List<AstNode> Body;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstBasicBlock(ISourceLocation sourceLocation)
            : base(sourceLocation)
        {
            Body = new List<AstNode>();
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstBasicBlock(ISourceLocation sourceLocation, IEnumerable<AstNode> body)
            : base(sourceLocation)
        {
            Body = (body != null) ? body.ToList() : new List<AstNode>();
        }

        /// <summary>
        /// Gets all direct child nodes.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            return Body;
        }

        /// <summary>
        /// Write human readable output.
        /// </summary>
        public override void WriteTo(ITextOutput output, FormattingOptions format)
        {
            output.Write("BasicBlock{");
            output.WriteLine();
            output.Indent();
            foreach (var child in GetChildren())
            {
                child.WriteTo(output, format);
                output.WriteLine();
            }
            output.Unindent();
            output.Write("}");
            output.WriteLine();
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