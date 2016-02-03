using System.Linq;
using System.Net;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// For instance fields, Will redirect calls to System.Threading.Interlocked 
    /// to an AtomicXXXFieldUpdater.
    /// For static fields, will surround all calls to System.Interlocked with a lock.
    /// It is important keep in mind to also surround the parameter 
    /// reference conversion performed by dot42.
    /// </summary>
    internal static class InterlockedConverter 
    {
        public static void Convert(AstNode ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            foreach (var block in ast.GetSelfAndChildrenRecursive<AstBlock>())
            {
                for (int i = 0; i < block.Body.Count; ++i)
                {
                    var expr = block.Body[i] as AstExpression;
                    if(expr == null) 
                        continue;


                    var interlockedPairs = expr.GetExpressionPairs(p =>
                                            p.Code == AstCode.Call
                                            && ((XMethodReference) p.Operand).DeclaringType.FullName == "System.Threading.Interlocked")
                                            .ToList();
                                                    
                    if(interlockedPairs.Count == 0) 
                        continue;
                    if (interlockedPairs.Count > 1)
                        throw new CompilerException("The Interlocked converter can not handle more than one interlocked call per statement. Try splittig the statement up.");

                    var interlockedCall = interlockedPairs.First().Expression;

                    // first parameter should be a reference to a field,
                    // (but be lenient if we don't find what we expect)
                    
                    var targetExpr = interlockedCall.Arguments[0];

                    XFieldReference field = null;

                    if (targetExpr.InferredType.IsByReference)
                    {
                        field = targetExpr.Operand as XFieldReference;

                        if (field != null)
                        {
                            // check if we have an atomic updater 
                            var updater = field.DeclaringType.Resolve().Fields
                                                             .FirstOrDefault(f => f.Name == field.Name + NameConstants.Atomic.FieldUpdaterPostfix);
                            if (updater != null)
                            {
                                var method = (XMethodReference) interlockedCall.Operand;
                                var methodName = method.Name.Split('$')[0]; // retrieve original name.

                                if (InterlockedUsingUpdater(interlockedCall, methodName, field, updater, targetExpr, interlockedPairs.First().Parent, compiler))
                                    continue;
                            }
                        }
                    }
                    

                    FailsafeInterlockedUsingLocking(field, expr, targetExpr, block, i, compiler, currentMethod);
                }

            }
        }

        private static bool InterlockedUsingUpdater(AstExpression interlockedCall, string methodName, XFieldReference field,
                                                    XFieldDefinition updater, AstExpression targetExpr, AstExpression parentExpr,
                                                    AssemblyCompiler compiler)
        {

            bool isStatic = field.Resolve().IsStatic;

            string replacementMethod = null;
            if (methodName == "Increment")
                replacementMethod = "IncrementAndGet";
            else if (methodName == "Decrement")
                replacementMethod = "DecrementAndGet";
            else if (methodName == "Add")
                replacementMethod = "AddAndGet";
            else if (methodName == "Read")
                replacementMethod = "Get";
            else if (methodName.StartsWith("Exchange"))
                replacementMethod = "GetAndSet";
            else if (methodName.StartsWith("CompareAndSet"))
                // this patches through to the original java method for performance purists.
                replacementMethod = "CompareAndSet"; 
            else if (methodName.StartsWith("CompareExchange"))
            {
                if (TransformCompareExchangeToCompareAndSet(parentExpr, compiler, ref interlockedCall))
                    replacementMethod = "CompareAndSet";
                else
                {
                    // The semantics here are slighlty different. Java returns a 'true' on 
                    // success, while BCL returns the old value. We have crafted a replacement 
                    // method with the BCL semantics though.
                    replacementMethod = "CompareExchange";
                }
            }
            else
            {
                return false;
            }

            var updaterType = updater.FieldType.Resolve();
            var methodRef = updaterType.Methods.FirstOrDefault(f => f.Name == replacementMethod);

            if (methodRef == null && methodName=="CompareExchange") 
                methodRef = updaterType.Methods.FirstOrDefault(f => f.Name.StartsWith(replacementMethod));

            if (methodRef == null)
                return false;

            interlockedCall.Operand = methodRef;

            if (isStatic)
                interlockedCall.Arguments[0] = new AstExpression(interlockedCall.SourceLocation, AstCode.Ldnull, null)
                                                    .SetType(field.DeclaringType);
            else
                interlockedCall.Arguments[0] = targetExpr.Arguments[0];

            // add field updater instance argument.
            var ldUpdater = new AstExpression(interlockedCall.SourceLocation, AstCode.Ldsfld, updater)
                                    .SetType(updaterType);

            interlockedCall.Arguments.Insert(0, ldUpdater);

            return true;

        }

        private static bool TransformCompareExchangeToCompareAndSet(AstExpression parentExpr, AssemblyCompiler compiler, ref AstExpression interlockedCall)
        {
            if (parentExpr == null) 
            {
                // as the return value is not used, transformation is possible.
                SwapArgumentsAndSetResultType(interlockedCall, compiler);
                return true; 
            }

            // Java returns a 'true' on  success, while BCL always returns the old value.
            // Assuming the comparants are the same, in BLC terms 'ceq' means checking for 
            // success (i.e. old value is expected value), while 'cne' means checking for 
            // failure (i.e. value found if different from expected value)
            // 'brfalse' a zero comparand mean branch on success, while brtrue means the opposite.

            var comparant1 = interlockedCall.Arguments[2];

            if (parentExpr.Code == AstCode.Brfalse || parentExpr.Code == AstCode.Brtrue)
            {
                // compare to not null.
                switch (comparant1.Code)
                {
                    case AstCode.Ldnull:
                        break;
                    case AstCode.Ldc_I4:
                    case AstCode.Ldc_I8:
                        {
                            if (System.Convert.ToInt64(comparant1.Operand) != 0)
                                return false;
                            break;
                        }
                    default:
                        return false;
                }

                // transformation is possible.
                parentExpr.Code = parentExpr.Code == AstCode.Brfalse ? AstCode.Brtrue : AstCode.Brfalse;
                SwapArgumentsAndSetResultType(interlockedCall, compiler);
                return true;
            }
            
            // Cne should work in theory, but I have not actually seen it so I was unable to test.
            // Leave it out for safety until actually encountered.
            if (   parentExpr.Code != AstCode.Ceq 
                // parentExpr.Code != AstCode.Cne
                && parentExpr.Code != AstCode.__Beq 
                && parentExpr.Code != AstCode.__Bne_Un) 
                return false;

            var comparand2 = parentExpr.Arguments[1];

            if (comparant1.Code != comparand2.Code || !Equals(comparant1.Operand, comparand2.Operand))
                return false;
            
            switch (comparant1.Code)
            {
                case AstCode.Ldloc:
                case AstCode.Ldnull:
                case AstCode.Ldc_I4:
                case AstCode.Ldc_I8:
                    break;
                default:
                    return false;
            }

            // transformation is possible.
            if (parentExpr.Code == AstCode.Ceq)
            {
                parentExpr.CopyFrom(interlockedCall);
                interlockedCall = parentExpr;
            }
            //else if (parentExpr.Code == AstCode.Cne) // see above.
            //{
            //    parentExpr.Code = AstCode.Not;
            //    parentExpr.Arguments.RemoveAt(1);
            //}
            else if (parentExpr.Code == AstCode.__Beq)
            {
                parentExpr.Code = AstCode.Brtrue;
                parentExpr.Arguments.RemoveAt(1);
            }
            else if (parentExpr.Code == AstCode.__Bne_Un)
            {
                parentExpr.Code = AstCode.Brfalse;
                parentExpr.Arguments.RemoveAt(1);
            }
            else
            {
                return false;
            }

            SwapArgumentsAndSetResultType(interlockedCall, compiler);
            return true;
        }

        private static void SwapArgumentsAndSetResultType(AstExpression interlockedCall, AssemblyCompiler compiler)
        {
            var arg1 = new AstExpression(interlockedCall.Arguments[1]);
            var arg2 = new AstExpression(interlockedCall.Arguments[2]);
            interlockedCall.Arguments[1].CopyFrom(arg2);
            interlockedCall.Arguments[2].CopyFrom(arg1);
            interlockedCall.ExpectedType = null;
            interlockedCall.InferredType = compiler.Module.TypeSystem.Bool;
        }

        private static void FailsafeInterlockedUsingLocking(XFieldReference field, AstExpression expr, 
                                                            AstExpression targetExpr, AstBlock block, int idx, 
                                                            AssemblyCompiler compiler, MethodSource currentMethod)
        {
            if (currentMethod.IsDotNet && !currentMethod.ILMethod.DeclaringType.HasSuppressMessageAttribute("InterlockedFallback"))
            {
                bool isStatic = field != null && field.Resolve().IsStatic;

                DLog.Warning(DContext.CompilerCodeGenerator,
                    "Emulating Interlocked call using failsafe locking mechanism in {0}{1}. Consider using AtomicXXX classes instead. You can suppress this message with an [SuppressMessage(\"dot42\", \"InterlockedFallback\")] attribute on the class.",
                    currentMethod.Method.FullName, !isStatic ? "" : " because a static field is referenced");
            }

            // replace with:
            //    Monitor.Enter();
            //    try { <original expression> } finally { Monitor.Exit(); } 
            //   
            // note that the lock is larger than it has to be, since it also sourrounds
            // the parameter evaluation.
            // It must sourround the storing and loading of the reference parameters though.
            var typeSystem = compiler.Module.TypeSystem;

            var monitorType = compiler.GetDot42InternalType("System.Threading", "Monitor");
            var enterMethod = monitorType.Resolve().Methods.Single(p => p.Name == "Enter");
            var exitMethod = monitorType.Resolve().Methods.Single(p => p.Name == "Exit");

            AstExpression loadLockTarget = null;
            
            if (field != null)
            {
                if (field.Resolve().IsStatic)
                {
                    // lock on the field's class typedef.
                    // but always the element type, not on a generic instance (until Dot42 implements proper generic static field handling)
                    loadLockTarget = new AstExpression(expr.SourceLocation, AstCode.LdClass, field.DeclaringType.GetElementType())
                        .SetType(typeSystem.Type);
                }
                else
                {
                    // lock on the fields object
                    loadLockTarget = targetExpr.Arguments[0];
                }
            }

            if (loadLockTarget == null)
            {
                // something went wrong. use a global lock.
                DLog.Warning(DContext.CompilerCodeGenerator, "unable to infer target of Interlocked call. using global lock.");
                var interlockedType = compiler.GetDot42InternalType("System.Threading", "Interlocked");
                loadLockTarget = new AstExpression(expr.SourceLocation, AstCode.LdClass, interlockedType)
                                        .SetType(typeSystem.Type);
            }

            var lockVar = new AstGeneratedVariable("lockTarget$", "") {Type = typeSystem.Object};
            var storeLockVar = new AstExpression(expr.SourceLocation, AstCode.Stloc, lockVar, loadLockTarget);
            var loadLockVar = new AstExpression(expr.SourceLocation, AstCode.Ldloc, lockVar);
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

            block.Body[idx] = replacementBlock;
        }
    }
}
