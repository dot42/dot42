using System;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert delegate ctors
    /// </summary>
    internal static class DelegateConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions(AstCode.Newobj))
            {
                var method = (XMethodReference) node.Operand;
                if (method.Name == ".ctor")
                {
                    XTypeDefinition declaringType;
                    if (method.DeclaringType.TryResolve(out declaringType) && declaringType.IsDelegate())
                    {
                        var ldftn = node.Arguments[1];
                        if ((ldftn.Code == AstCode.Ldftn) || (ldftn.Code == AstCode.Ldvirtftn))
                        {
                            node.Code = AstCode.Delegate;

                            var token = (XMethodReference)ldftn.Operand;
                            node.Operand = Tuple.Create(declaringType, token);
                            node.Arguments.RemoveAt(1);
                        }
                    }
                }
            }
        }
    }
}
