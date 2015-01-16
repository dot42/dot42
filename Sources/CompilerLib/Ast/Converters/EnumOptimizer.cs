using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Optimize enum conversions
    /// </summary>
    internal static class EnumOptimizer
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            // Optimize enum2int(ldsfld(enum-const))
            foreach (var node in ast.GetExpressions())
            {
                switch (node.Code)
                {
                    case AstCode.Enum_to_int:
                    case AstCode.Enum_to_long:
                        {
                            var arg = node.Arguments[0];
                            XFieldReference fieldRef;
                            if (arg.Match(AstCode.Ldsfld, out fieldRef))
                            {
                                XFieldDefinition field;
                                object value;
                                if (fieldRef.TryResolve(out field) && field.IsStatic && field.DeclaringType.IsEnum && field.TryGetEnumValue(out value))
                                {
                                    // Replace with ldc_ix
                                    var wide = (node.Code == AstCode.Enum_to_long);
                                    node.SetCode(wide ? AstCode.Ldc_I8 : AstCode.Ldc_I4);
                                    node.Operand = wide ? (object)XConvert.ToLong(value) : XConvert.ToInt(value);
                                    node.Arguments.Clear();
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}