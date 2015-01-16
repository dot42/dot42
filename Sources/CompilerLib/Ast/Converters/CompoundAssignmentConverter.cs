using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert by compound assignment ( a += y )
    /// </summary>
    internal static class CompoundAssignmentConverter 
    {
        private static readonly Dictionary<AstCode,AstCode> operatorMap = new Dictionary<AstCode, AstCode> {
            { AstCode.Add, AstCode.CompoundAdd },
            { AstCode.Sub, AstCode.CompoundSub },
            { AstCode.Mul, AstCode.CompoundMul },
            { AstCode.Div, AstCode.CompoundDiv },
            { AstCode.Rem, AstCode.CompoundRem },
            { AstCode.And, AstCode.CompoundAnd },
            { AstCode.Or, AstCode.CompoundOr },
            { AstCode.Xor, AstCode.CompoundXor },
            { AstCode.Shl, AstCode.CompoundShl },
            { AstCode.Shr, AstCode.CompoundShr },
            { AstCode.Shr_Un, AstCode.CompoundShr_Un },
        };
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions(AstCode.Stloc))
            {
                // Select the nodes to work on
                var ldCode = AstCode.Ldloc;

                var stArg = node.Arguments[0];
                AstCode newCode;
                if ((stArg.Arguments.Count >= 1) && (operatorMap.TryGetValue(stArg.Code, out newCode)))
                {
                    // Found matching operator
                    var ldArg = stArg.Arguments[0];
                    if (ldArg.Code == ldCode)
                    {
                        // Found a matching 'load' opcode.
                        if (ldArg.Operand == node.Operand)
                        {
                            var type = stArg.GetResultType();
                            if (!type.IsEnum())
                            {
                                // Found matching "local" 
                                node.Code = newCode;
                                node.SetArguments(stArg.Arguments.Skip(1));
                                node.SetType(type);
                            }
                        }
                    }
                }
            }
        }
    }
}
