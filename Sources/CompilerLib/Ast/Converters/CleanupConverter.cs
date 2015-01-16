using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Perform last minute cleanup.
    /// </summary>
    internal static class CleanupConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions())
            {
                switch (node.Code)
                {
                    case AstCode.ByRefOutArray:
                        node.Arguments.Clear();
                        return;
                    case AstCode.Conv_U2:
                        if (node.GetResultType().IsChar() && node.Arguments[0].Match(AstCode.Int_to_ushort))
                        {
                            // Remove useless conversion (int_to_ushort followed by conv_u2)
                            var value = node.Arguments[0].Arguments[0];
                            node.CopyFrom(value);
                        }
                        break;
                }
            }
        }
    }
}
