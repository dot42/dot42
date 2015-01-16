namespace Dot42.CompilerLib.Ast
{
    public sealed class AstExpressionPair
    {
        /// <summary>
        /// The expression
        /// </summary>
        public readonly AstExpression Expression;

        /// <summary>
        /// The parent of <see cref="Expression"/> (or null if there is no such parent).
        /// </summary>
        public readonly AstExpression Parent;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AstExpressionPair(AstExpression expression, AstExpression parent)
        {
            Expression = expression;
            Parent = parent;
        }
    }
}
