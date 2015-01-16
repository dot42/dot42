using System;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ldc_I4,ldc_R4 with wide expected type to ldc_I8,ldc_I8
    /// </summary>
    internal static class LdcWideConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions())
            {
                if ((node.Code == AstCode.Conv_U8) && (node.Arguments[0].Code == AstCode.Ldc_I4))
                {
                    var value = node.Arguments[0].Operand;
                    node.Code = AstCode.Ldc_I8;
                    node.Arguments.Clear();
                    node.Operand = XConvert.ToLong(value) & 0xFFFFFFFFL;
                }
                else if ((node.Code == AstCode.Ldc_I4) || (node.Code == AstCode.Ldc_R4))
                {
                    var type = node.GetResultType();
                    if (type.IsWide())
                    {
                        node.Code = (node.Code == AstCode.Ldc_I4) ? AstCode.Ldc_I8 : AstCode.Ldc_R8;
                    }
                }
            }
        }
    }
}
