using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Jump target
    /// </summary>
    public sealed class AstLabel: AstNode
    {
        public readonly string Name;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstLabel(ISourceLocation sourceLocation, string name)
            : base(sourceLocation)
        {
            Name = name;
        }

        /// <summary>
        /// Write human readable output.
        /// </summary>
        public override void WriteTo(ITextOutput output)
        {
            output.WriteDefinition(Name + ":", this);
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