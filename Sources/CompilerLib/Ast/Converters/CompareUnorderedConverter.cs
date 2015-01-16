using System;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert Cxx_Un
    /// </summary>
    internal static class CompareUnorderedConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions(AstCode.Cgt_Un))
            {
                if ((node.Arguments.Count == 2) && (node.Arguments[1].Code == AstCode.Ldnull))
                {
                    if (node.Arguments[0].Code == AstCode.Isinst)
                    {
                        // Cgt_un(IsInst(x), ldnull) -> InstanceOf(x)
                        var argX = node.Arguments[0].Arguments[0];
                        node.Operand = node.Arguments[0].Operand;
                        node.Code = AstCode.InstanceOf;
                        node.Arguments.Clear();
                        node.Arguments.Add(argX);
                    }
                    else
                    {
                        // Cgt_un(x, ldnull) -> CIsNotNull(x)
                        node.Arguments.RemoveAt(1);
                        node.Code = AstCode.CIsNotNull;
                    }
                }
            }
        }
    }
}
