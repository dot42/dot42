using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Dot42.CompilerLib.Ast.Extensions;
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
            foreach (var node in ast.GetExpressions())
            {
                switch (node.Code)
                {
                    // Optimize enum2int(ldsfld(enum-const))
                    //      and enum2int(int.to.enum(xxx))
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
                            else if (arg.Code == AstCode.Int_to_enum || arg.Code == AstCode.Long_to_enum)
                            {
                                var expectedType = node.ExpectedType;
                                node.CopyFrom(arg.Arguments[0]);
                                node.ExpectedType = expectedType;
                            }
                        }
                        break;
                    // optimize ceq/cne (int/long-to-enum(int), yyy)
                    case AstCode.Ceq:
                    case AstCode.Cne:
                    {
                        if (node.Arguments.Any(a=>IsToEnum(a.Code)))
                        {
                            // xx_to_enum is a quite costly operation when compared to enum-to-int,
                            // so convert this to an interger-only compare.
                            bool isLong = node.Arguments.Any(a => a.Code == AstCode.Long_to_enum);

                            foreach (var arg in node.Arguments)
                            {
                                if (IsToEnum(arg.Code))
                                {
                                    arg.CopyFrom(arg.Arguments[0]);
                                    arg.ExpectedType = isLong ? compiler.Module.TypeSystem.Long : compiler.Module.TypeSystem.Int;
                                }
                                else
                                {
                                    Debug.Assert(arg.GetResultType().IsEnum());
                                    var orig = new AstExpression(arg);
                                    var convert = new AstExpression(arg.SourceLocation,
                                        isLong ? AstCode.Enum_to_long : AstCode.Enum_to_int, null, orig);
                                    convert.ExpectedType = isLong ? compiler.Module.TypeSystem.Long : compiler.Module.TypeSystem.Int;
                                    arg.CopyFrom(convert);
                                }
                            }
                        }
                        break;
                    }
                }
            }
 
        }

        private static bool IsToEnum(AstCode code)
        {
            return code == AstCode.Int_to_enum || code == AstCode.Long_to_enum;
        }

    }
}