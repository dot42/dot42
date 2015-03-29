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

                            if (value.GetResultType().IsChar())
                            {
                                node.CopyFrom(value);
                            }
                            else
                            {
                                // keep the conv_u2 but drop the int_to_ushort.
                                // TODO:maybe there is a better way to do the conversion in one step.
                                // Convert.ToUInt16(char) looks now like this:

                                // .method public static ToUInt16(C)S
                                //    .registers 3
                                //    .param p0    # C

                                //    #v0=(Uninit);p0=(Char);
                                //    int-to-char v0, p0
                                //    #v0=(Char);

                                //    #v1=(Uninit);
                                //    const v1, 0xffff
                                //    #v1=(Char);

                                //    #v0=(Char);v1=(Char);
                                //    and-int/2addr v0, v1
                                //    #v0=(Integer);

                                //    #v0=(Integer);
                                //    return v0
                                //.end method

                                node.Arguments.Clear();
                                node.Arguments.Add(value);
                            }
                        }
                        break;
                }
            }
        }
    }
}
