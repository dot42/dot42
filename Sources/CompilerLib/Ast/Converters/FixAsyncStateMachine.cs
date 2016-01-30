using System.Linq;
using System.Net;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.XModel;
using Dot42.Utility;
using java.time.temporal;

namespace Dot42.CompilerLib.Ast.Converters
{
    /// <summary>
    /// Fixes using 'lock's in async methods.
    /// 
    /// The ART dex2oat verifier seems to be very keen to detect misuses of
    /// monitor-enter / monitor-exit. See 
    /// https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-2.html#jvms-2.11.10,
    /// https://code.google.com/p/android/issues/detail?id=80823, 
    /// (http://stackoverflow.com/questions/4201713/synchronization-vs-lock) 
    /// for details.
    /// 
    /// This breaks the MoveNext method of the StateMachine that is created for 
    /// async methods. In this method a "<>do_finallyBodies" local variable, or,
    /// with VS 2015/Roslyn compiler, a field of the state machine class is checked 
    /// prior to executing a finally block if is is still to be executed.
    /// When using locks in async methods, this behavior breaks the ART verifier, 
    /// who rejects the methods on grounds that the method may not call 
    /// an 'monitor_exit' belonging to the 'monitor_enter'.
    /// 
    /// We know that the variable/field will not change between entering the
    /// 'lock' block and executing the finally block. As a workaround for the
    /// overzealous ART verifier, we therefore remove the finally-check when 
    /// using locks.
    /// </summary>
    internal static class FixAsyncStateMachine
    {
        public static void Convert(AstNode ast, MethodSource currentMethod, AssemblyCompiler compiler)
        {
            // only work on MoveNext of IAsyncStateMachine implementations.
            if (currentMethod.Name != "MoveNext" && currentMethod.Name != "IAsyncStateMachine_MoveNext")
                return;
            var declaringType = currentMethod.Method.DeclaringType.Resolve();
            if (declaringType.Interfaces.All(i => i.FullName != "System.Runtime.CompilerServices.IAsyncStateMachine"))
                return;

            foreach(var block in ast.GetSelfAndChildrenRecursive<AstBlock>())
                FixBlock(block);
        }

        private static void FixBlock(AstBlock block)
        {
            if (block == null) return;

            bool wasLastExpressionCallToMonitorEnter = false;

            foreach (AstNode node in block.GetChildren())
            {
                var expr = node as AstExpression;
                if (expr != null)
                {
                    if(expr.Code != AstCode.Nop)
                        wasLastExpressionCallToMonitorEnter = IsCallToMonitorEnter(expr);
                    continue;
                }

                var tryCatch = node as AstTryCatchBlock;
                if (wasLastExpressionCallToMonitorEnter && tryCatch != null)
                {
                    // a lock statement.
                    FixTryCatchFinally(tryCatch);
                }

                wasLastExpressionCallToMonitorEnter = false;
            }
        }

        private static void FixTryCatchFinally(AstTryCatchBlock node)
        {
            // this of course is all rather brittle and tied to the 
            // internal workings of the C# compiler.

            if (node.FinallyBlock == null)
                return;
            var firstExpr = node.FinallyBlock.GetChildren().First() as AstExpression;
            if (firstExpr == null)
                return;
            if (!firstExpr.Code.IsConditionalControlFlow())
                return;

            if (firstExpr.Arguments[0].Code != AstCode.Ldloc) // should read a local variable
                return;
            var condVar = firstExpr.Arguments[0].Operand as AstVariable;
            if (condVar == null)  // safety check needed?
                return;
            // make sure the variable is not written to in the try block or any of the catch blocks
            if(IsModified(node.TryBlock, condVar) || node.CatchBlocks.Any(c=>IsModified(c, condVar)))
                throw  new CompilerException("Unable to fix complex locking in async method.");

            // remove the conditional branch.
            firstExpr.Arguments.Clear();
            firstExpr.Operand = null;
            firstExpr.Code = AstCode.Nop;
        }

        private static bool IsModified(AstBlock block, AstVariable var)
        {
            return block.GetExpressions(AstCode.Stloc)
                        .Any(a => a.Operand == var);
        }

        private static bool IsCallToMonitorEnter(AstExpression expr)
        {
            if(expr.Code != AstCode.Call) return false;
            var method = (XMethodReference)expr.Operand;

            return method.Name == "Enter"
                && method.DeclaringType.FullName == "System.Threading.Monitor";
        }
    }
}
