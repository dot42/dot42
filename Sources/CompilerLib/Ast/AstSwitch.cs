using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Switch statement
    /// </summary>
    public sealed class AstSwitch : AstNode
    {
        public AstExpression Condition;
        public List<CaseBlock> CaseBlocks = new List<CaseBlock>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstSwitch(ISourceLocation sourceLocation)
            : base(sourceLocation)
        {
        }

        /// <summary>
        /// Gets all direct child nodes.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            if (Condition != null)
                yield return Condition;
            foreach (var caseBlock in CaseBlocks)
            {
                yield return caseBlock;
            }
        }

        /// <summary>
        /// Write human readable output.
        /// </summary>
        public override void WriteTo(ITextOutput output)
        {
            output.Write("switch (");
            Condition.WriteTo(output);
            output.WriteLine(") {");
            output.Indent();
            foreach (var caseBlock in CaseBlocks)
            {
                caseBlock.WriteTo(output);
            }
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

        /// <summary>
        /// Block for a specific case
        /// </summary>
        public sealed class CaseBlock : AstBlock
        {
            public List<int> Values;  // null for the default case

            /// <summary>
            /// Default ctor
            /// </summary>
            public CaseBlock(ISourceLocation sourceLocation)
                : this(sourceLocation, null)
            {
            }

            /// <summary>
            /// Ctor
            /// </summary>
            public CaseBlock(ISourceLocation sourceLocation, IEnumerable<AstNode> body)
                : base(sourceLocation)
            {
                Body = (body != null) ? body.ToList() : new List<AstNode>();
            }

            /// <summary>
            /// Write human readable output.
            /// </summary>
            public override void WriteTo(ITextOutput output)
            {
                if (Values != null)
                {
                    foreach (var i in Values)
                    {
                        output.WriteLine("case {0}:", i);
                    }
                }
                else
                {
                    output.WriteLine("default:");
                }
                output.Indent();
                base.WriteTo(output);
                output.Unindent();
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