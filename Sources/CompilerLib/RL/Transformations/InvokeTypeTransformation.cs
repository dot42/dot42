using Dot42.DexLib;
using Dot42.DexLib.Extensions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Make sure that invoke uses the correct postfix.
    /// </summary>
    internal class InvokeTypeTransformation : IRLTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        public bool Transform(Dex target, MethodBody body)
        {
            foreach (var ins in body.Instructions)
            {
                switch (ins.Code)
                {
                    case RCode.Invoke_direct:
                    case RCode.Invoke_virtual:
                    case RCode.Invoke_interface:
                        MethodDefinition method;
                        if (((MethodReference)ins.Operand).TryResolve(target, out method))
                        {
                            if (method.Owner.IsInterface)
                            {
                                ins.Code = RCode.Invoke_interface;
                            }
                            else if (method.IsStatic) // This happens for android extension methods. 
                                                      // Is this a hack. why was the correct invoke code not 
                                                      // used in the first place?
                                                      // (in AstCompilerVisitor.Expression.VisitCallExpression?)
                            {
                                ins.Code = RCode.Invoke_static;
                            }
                            else if (method.IsDirect)
                            {
                                ins.Code = RCode.Invoke_direct;
                            }
                            else
                            {
                                ins.Code = RCode.Invoke_virtual;
                            }
                        }
                        break;
                }
            }

            return false;
        }
    }
}
