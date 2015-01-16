namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Prefix instruction
    /// </summary>
    public sealed class AstExpressionPrefix
    {
        public readonly AstCode Code;
        public readonly object Operand;
		
        /// <summary>
        /// Default ctor
        /// </summary>
        public AstExpressionPrefix(AstCode code, object operand = null)
        {
            Code = code;
            Operand = operand;
        }
    }
}