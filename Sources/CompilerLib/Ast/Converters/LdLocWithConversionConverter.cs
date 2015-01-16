using System;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Convert ldloc with a different expected type then it's variable.
    /// </summary>
    internal static class LdLocWithConversionConverter 
    {
        /// <summary>
        /// Optimize expressions
        /// </summary>
        public static void Convert(AstNode ast)
        {
            foreach (var node in ast.GetExpressions())
            {
                if (node.Code == AstCode.Ldloc)
                {
                    var variable = (AstVariable) node.Operand;
                    var varType = variable.Type;
                    var expType = node.ExpectedType;
                    if ((expType != null) && (!varType.IsSame(expType)))
                    {
                        if (varType.IsByte() && (expType.IsChar() || expType.IsUInt16()))
                        {
                            var ldloc = new AstExpression(node);
                            var code = expType.IsUInt16() ? AstCode.Int_to_ushort : AstCode.Conv_U2;
                            node.CopyFrom(new AstExpression(node.SourceLocation, code, null, ldloc).SetType(expType));
                        } 
                    }
                }
                else if (node.Code.IsCall())
                {
                    var methodRef = (XMethodReference) node.Operand;
                    var returnType = methodRef.ReturnType;
                    var expType = node.ExpectedType;
                    if ((expType != null) && (!returnType.IsSame(expType)))
                    {
                        if (returnType.IsByte() && (expType.IsChar() || expType.IsUInt16()))
                        {
                            var ldloc = new AstExpression(node);
                            var code = expType.IsUInt16() ? AstCode.Int_to_ushort : AstCode.Conv_U2;
                            node.CopyFrom(new AstExpression(node.SourceLocation, code, null, ldloc).SetType(expType));
                        }
                        else if (returnType.IsUInt16() && expType.IsChar())
                        {
                            var ldloc = new AstExpression(node);
                            node.CopyFrom(new AstExpression(node.SourceLocation, AstCode.Conv_U2, null, ldloc).SetType(expType));
                        }
                        else if (returnType.IsChar() && expType.IsUInt16())
                        {
                            var ldloc = new AstExpression(node);
                            node.CopyFrom(new AstExpression(node.SourceLocation, AstCode.Int_to_ushort, null, ldloc).SetType(expType));
                        }
                    }
                }
            }
        }
    }
}
