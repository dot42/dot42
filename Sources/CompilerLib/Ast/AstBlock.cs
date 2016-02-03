using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// List of ast nodes.
    /// </summary>
    public class AstBlock : AstNode
    {
        public AstExpression EntryGoto;
        public List<AstNode> Body;

        /// <summary>
        /// Is set, this block will not be converted to targets anymore
        /// </summary>
        public bool IsOptimizedForTarget { get; set; }

        public AstBlock(ISourceLocation sourceLocation, params AstNode[] body)
            : this(sourceLocation, (IEnumerable<AstNode>)body)
        {
        }

        public AstBlock(ISourceLocation sourceLocation, IEnumerable<AstNode> body)
            : base(sourceLocation)
        {
            Body = (body != null) ? body.ToList() : new List<AstNode>();
        }

        public AstBlock(List<AstNode> body)
            : base((body != null) ? body.Select(x => x.SourceLocation).FirstOrDefault() : null)
        {
            Body = (body != null) ? body.ToList() : new List<AstNode>();
        }

        /// <summary>
        /// Helper
        /// </summary>
        public static AstBlock Create<T>(ISourceLocation sourceLocation, params T[] nodes)
            where T : AstNode
        {
            return new AstBlock(sourceLocation, nodes.Cast<AstNode>());
        }

        /// <summary>
        /// Helper
        /// </summary>
        public static AstBlock Create<T>(params T[] nodes)
            where T : AstNode
        {
            return new AstBlock(NoSource, nodes.Cast<AstNode>());
        }

        /// <summary>
        /// Helper
        /// </summary>
        public static AstBlock CreateOptimizedForTarget<T>(ISourceLocation sourceLocation, params T[] nodes)
            where T : AstNode
        {
            return new AstBlock(sourceLocation, nodes.Cast<AstNode>()) { IsOptimizedForTarget = true };
        }

        /// <summary>
        /// Helper
        /// </summary>
        public static AstBlock CreateOptimizedForTarget<T>(params T[] nodes)
            where T : AstNode
        {
            return new AstBlock(NoSource, nodes.Cast<AstNode>()) { IsOptimizedForTarget = true };
        }

        public override IEnumerable<AstNode> GetChildren()
        {
            if (EntryGoto != null)
                yield return EntryGoto;
            foreach (var child in Body)
            {
                yield return child;
            }
        }

        public override void WriteTo(ITextOutput output, FormattingOptions format)
        {
            output.Write("Block{");
            output.WriteLine();
            if (EntryGoto != null)
            {
                output.Indent();
                output.Write("// Entry Goto");
                output.WriteLine();
                EntryGoto.WriteTo(output, format);
                output.Unindent();
            }
            if (Body != null)
            {
                output.Indent();
                output.Write("// Body");
                output.WriteLine();
                foreach (AstNode child in Body)
                {
                    child.WriteTo(output, format);
                    output.WriteLine();
                }
                output.Unindent();
            }
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