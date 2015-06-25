using System.Linq;
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

                    var interlockedCall = expr.GetSelfAndChildrenRecursive<AstExpression>(p=>
                                                        p.Code == AstCode.Call 
                                                        && ((XMethodReference)p.Operand).DeclaringType.FullName == "System.Threading.Interlocked")
                                                    .FirstOrDefault();
                    if(interlockedCall == null) 
                        continue;

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
                                if (InterlockedUsingUpdater(interlockedCall, field, updater, targetExpr, compiler))
                                    continue;
                            }
                        }
                    }
                    

                    if (currentMethod.IsDotNet && !currentMethod.ILMethod.DeclaringType.HasSuppressMessageAttribute("InterlockedFallback"))
                    {
                        bool isStatic = field != null && field.Resolve().IsStatic;

                        DLog.Warning(DContext.CompilerCodeGenerator,
                            "Emulating Interlocked call using failsafe locking mechanism in {0}{1}. Consider using AtomicXXX classes instead. You can suppress this message with an [SuppressMessage(\"dot42\", \"InterlockedFallback\")] attribute on the class.",
                            currentMethod.Method.FullName, !isStatic ? "" : " because a static field is referenced");
                    }
                    FailsafeInterlockedUsingLocking(field, expr, targetExpr, block, i, compiler);
                }

            }
        }

        private static bool InterlockedUsingUpdater(AstExpression interlockedCall, XFieldReference field, XFieldDefinition updater, AstExpression targetExpr, AssemblyCompiler compiler)
        {
            var method = (XMethodReference) interlockedCall.Operand;

            bool isStatic = field.Resolve().IsStatic;

            var methodName = method.Name.Split('$')[0]; // retrieve original name.
            
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
                // The semantics here are slighlty different. Java returns a 'true' on 
                // success, while BCL returns the old value. We have crafted a replacement 
                // method with the BCL semantics though.
                replacementMethod = "CompareExchange";
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

        private static void FailsafeInterlockedUsingLocking(XFieldReference field, AstExpression expr, 
                                                            AstExpression targetExpr, AstBlock block, int idx, 
                                                            AssemblyCompiler compiler)
        {
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
