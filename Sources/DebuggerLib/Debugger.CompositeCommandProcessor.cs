using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.DebuggerLib.Model;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Dispatcher of JDWP events.
    /// </summary>
    partial class Debugger
    {
        private readonly CompositeCommandProcessor compositeCommandProcessor;

        /// <summary>
        /// Return values: true == is handled, false == is not handled
        /// </summary>
        private class CompositeCommandProcessor : EventVisitor<bool, Jdwp.SuspendPolicy>
        {
            private readonly Debugger debugger;

            /// <summary>
            /// Default ctor
            /// </summary>
            public CompositeCommandProcessor(Debugger debugger)
            {
                this.debugger = debugger;
            }

            /// <summary>
            /// Notify the debug process of our suspension.
            /// </summary>
            private void HandleSuspendPolicy(Jdwp.SuspendPolicy suspendPolicy, SuspendReason reason, DalvikThread thread)
            {
                if (suspendPolicy == Jdwp.SuspendPolicy.All) 
                {
                    debugger.Process.OnSuspended(reason, thread);
                }                
            }

            /// <summary>
            /// Catch all handler.
            /// </summary>
            protected override bool Visit(JdwpEvent e, Jdwp.SuspendPolicy suspendPolicy)
            {
                HandleSuspendPolicy(suspendPolicy, SuspendReason.ProcessSuspend, null);
                return base.Visit(e, suspendPolicy);
            }

            /// <summary>
            /// Forward breakpoint events
            /// </summary>
            public override bool Visit(Breakpoint e, Jdwp.SuspendPolicy suspendPolicy)
            {
                DalvikThread thread;
                debugger.process.ThreadManager.TryGet(e.ThreadId, out thread);
                HandleSuspendPolicy(suspendPolicy, SuspendReason.Breakpoint, thread);
                debugger.Process.BreakpointManager.OnBreakpointEvent(e);
                return true;
            }

            /// <summary>
            /// Handle class prepare
            /// </summary>
            public override bool Visit(ClassPrepare e, Jdwp.SuspendPolicy data)
            {
                // Do not handle suspend policy here because the next will will do so.
                debugger.process.ReferenceTypeManager.OnClassPrepare(e);
                return true;
            }

            /// <summary>
            /// Handle caught/thrown exceptions
            /// </summary>
            public override bool Visit(Exception e, Jdwp.SuspendPolicy data)
            {
                DalvikThread thread;
                debugger.process.ThreadManager.TryGet(e.ThreadId, out thread);
                HandleSuspendPolicy(Jdwp.SuspendPolicy.All, SuspendReason.Exception, thread);
                debugger.Process.ExceptionManager.OnExceptionEvent(e, thread);
                return true;
            }

            /// <summary>
            /// Handle single step events
            /// </summary>
            public override bool Visit(SingleStep e, Jdwp.SuspendPolicy suspendPolicy)
            {
                var step = debugger.process.StepManager.GetAndRemove(e.RequestId);
                var thread = (step != null) ? step.Thread : null;
                var reason = (thread != null) ? SuspendReason.SingleStep : SuspendReason.ProcessSuspend;
                if (step != null)
                {
                    // Reset event
                    debugger.EventRequest.ClearAsync(Jdwp.EventKind.SingleStep, step.RequestId);
                }
                HandleSuspendPolicy(suspendPolicy, reason, thread);
                return true;
            }
        }
    }
}
