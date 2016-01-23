using System;
using System.Collections.Generic;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast2RLCompiler
{
    /// <summary>
    /// Manage Monitor.Enter / Monitor.Exit in a method.
    /// 
    /// The ART dex2oat verifier seems to be very keen to detect misuses of
    /// monitor-enter / monitor-exit. See 
    /// https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-2.html#jvms-2.11.10,
    /// https://code.google.com/p/android/issues/detail?id=80823, 
    /// (http://stackoverflow.com/questions/4201713/synchronization-vs-lock) 
    /// for details.
    /// 
    /// Most notably, in Android Lollipop (5.x) it checks if the register (!) used for
    /// a monitor-enter / monitor-exit pair is the very same. This is obviously overzealous,
    /// as the locked object matters, not the register.
    /// Nevertheless, here we try to keep dex2oat happy by tracking when a field is loaded/stored 
    /// to be used in Monitor.Enter/Exit. This typically happens in async methods.
    /// We try to re-use the same register in this case.
    /// 
    /// It is well possible that the current code does not work in all thinkable cases. 
    /// </summary>
    internal sealed class MonitorManager
    {
        private readonly Dictionary<object, Register> _monitorUsage = new Dictionary<object, Register>();

        public RLRange Enter(AstCompilerVisitor compVisit, AstExpression node, List<RLRange> args)
        {
            var reg = GetMonitorRegister(compVisit, node);
            if (reg != null)
            {
                var first = compVisit.Add(node.SourceLocation, RCode.Move_object, reg, args[0].Result);
                var second = compVisit.Add(node.SourceLocation, RCode.Monitor_enter, reg);
                return new RLRange(first, second, null);   
            }

            // default handling
            return new RLRange(compVisit.Add(node.SourceLocation, RCode.Monitor_enter, args[0].Result), null);
        }

        public RLRange Exit(AstCompilerVisitor compVisit, AstExpression node, List<RLRange> args)
        {
            var reg = GetMonitorRegister(compVisit, node);
            if (reg != null)
            {
                var first = compVisit.Add(node.SourceLocation, RCode.Move_object, reg, args[0].Result);
                var second = compVisit.Add(node.SourceLocation, RCode.Monitor_exit, reg);
                return new RLRange(first, second, null);
            }

            // default handling
            return new RLRange(compVisit.Add(node.SourceLocation, RCode.Monitor_exit, args[0].Result), null);
        }

        public Register GetMonitorRegister(AstCompilerVisitor compVisit, AstExpression node)
        {
            // this code is still experimental, and possibly does not handle all cases.
            var operand = node.Arguments[0].Operand;
            Register reg = null;

            if (operand is XFieldReference)
            {
                if (!_monitorUsage.TryGetValue(operand, out reg))
                {
                    reg = compVisit.Frame.AllocateTemp(FrameworkReferences.Object);
                    _monitorUsage.Add(operand, reg);
                }
            }
            return reg;
        }
    }
}
