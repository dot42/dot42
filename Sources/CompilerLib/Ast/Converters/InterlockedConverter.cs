using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Dot42.CompilerLib.XModel;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Will surround all calls to System.Interlocked with a lock.
    /// Important is to also sourround the parameter reference conversion
    /// performed by dot42.
    /// for instance-field references this lock will be held on the
    /// containing class, for static fields on the classes type.
    /// This is only an intermediate solution. See my comments on the
    /// System.Interlocked class for details.
    /// </summary>
    internal static class InterlockedConverter 
    {
        public static void Convert(AstNode ast, AssemblyCompiler compiler)
        {
            foreach (var block in ast.GetSelfAndChildrenRecursive<AstBlock>())
            {
                for (int i = 0; i < block.Body.Count; ++i)
                {
                    var expr = block.Body[i] as AstExpression;
                    if(expr == null) 
                        continue;

                    var interlockedCall = expr.GetSelfAndChildrenRecursive<AstExpression>(p=>
                                                        p.Code == AstCode.Call 
                                                        && ((XMethodReference)p.Operand).DeclaringType.FullName == "System.Threading.Interlocked")
                                                    .FirstOrDefault();
                    if(interlockedCall == null) 
                        continue;

                    // replace with:
                    //    Monitor.Enter();
                    //    try { <original expression> } finally { Monitor.Exit(); } 
                    //   
                    // note that the lock is larger than it has to be, since it also sourrounds
                    // the parameter evaluation.
                    // It must sourround the storing and loading of the reference parameters though.

                    // first parameter should be a reference to a field,
                    // (but be lenient if we don't find what we expect)
                    var monitorType = compiler.GetDot42InternalType("System.Threading", "Monitor");
                    var enterMethod = monitorType.Resolve().Methods.Single(p => p.Name == "Enter");
                    var exitMethod = monitorType.Resolve().Methods.Single(p => p.Name == "Exit");

                    
                    var target = interlockedCall.Arguments[0];
                    AstExpression loadLockTarget = null;

                    if (target.InferredType.IsByReference)
                    {
                        var field = target.Operand as XFieldReference;
                        if (field != null)
                        {
                            if (field.Resolve().IsStatic)
                            {
                                // lock on the field's class typedef.
                                loadLockTarget = new AstExpression(expr.SourceLocation, AstCode.LdClass, field.DeclaringType);
                            }
                            else
                            {
                                // lock on the fields object
                                loadLockTarget = target.Arguments[0];
                            }
                        }
                    }

                    if (loadLockTarget == null)
                    {
                        // somethin went wrong. use a global lock.
                        DLog.Warning(DContext.CompilerCodeGenerator, "unable to infer target of Interlocked call. using global lock.");
                        loadLockTarget = new AstExpression(expr.SourceLocation, AstCode.LdClass, monitorType);    
                    }

                    var lockVar = new AstGeneratedVariable("lockTarget$", "") {Type = compiler.Module.TypeSystem.Object};
                    var storeLockVar = new AstExpression(expr.SourceLocation, AstCode.Stloc, lockVar, loadLockTarget);
                    var loadLockVar  = new AstExpression(expr.SourceLocation, AstCode.Ldloc, lockVar);
                    var enterCall = new AstExpression(expr.SourceLocation, AstCode.Call, enterMethod, storeLockVar);

                    var replacementBlock = new AstBlock(expr.SourceLocation);
                    replacementBlock.Body.Add(enterCall);

                    var tryCatch = new AstTryCatchBlock(expr.SourceLocation)
                    {
                        TryBlock = new AstBlock(expr.SourceLocation, expr),
                        FinallyBlock = new AstBlock(expr.SourceLocation,
                            new AstExpression(block.SourceLocation, AstCode.Call, exitMethod, loadLockVar))
                    };

                    replacementBlock.Body.Add(tryCatch);

                    if (block.EntryGoto == expr)
                        block.EntryGoto = enterCall;

                    block.Body[i] = replacementBlock;

                }

            }
        }
    }
}
