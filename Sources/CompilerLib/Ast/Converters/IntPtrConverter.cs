using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ldloca, ldflda, lfsflda, addressof in the context of call arguments for non-primitive structs.
    /// </summary>
    internal static class IntPtrConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            // Convert IntPtr.Zero
            foreach (var node in ast.GetExpressions(AstCode.Ldsfld))
            {
                var field = (XFieldReference)node.Operand;
                if (field.DeclaringType.IsIntPtr() && (field.Name == "Zero"))
                {
                    node.Code = AstCode.Ldnull;
                    node.Operand = null;
                    node.Arguments.Clear();
                    node.SetType(compiler.Module.TypeSystem.Object);
                }                
            }

            // Convert box(IntPtr)
            foreach (var node in ast.GetExpressions(AstCode.Box))
            {
                var type = (XTypeReference)node.Operand;
                if (type.IsIntPtr())
                {
                    node.CopyFrom(node.Arguments[0]);
                }
            }
        }
    }
}
