using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert typeof(x) constructions
    /// </summary>
    internal static class TypeOfConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            foreach (var node in ast.GetExpressions())
            {
                AstExpression arg1;
                XMethodReference method;
                if (node.Match(AstCode.Call, out method, out arg1) && arg1.Match(AstCode.Ldtoken))
                {
                    if ((method.Name == "GetTypeFromHandle") && method.DeclaringType.IsSystemType())
                    {
                        node.Code = AstCode.TypeOf;
                        node.Operand = arg1.Operand;
                        node.Arguments.Clear();
                        node.SetType(compiler.Module.TypeSystem.Type);
                    }
                }
            }
        }
    }
}
