using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    public sealed class DebugBreakpointManager : DalvikBreakpointManager
    {
        private readonly DebugProgram program;
        private readonly EngineEventCallback eventCallback;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugBreakpointManager(DebugProgram program, EngineEventCallback eventCallback)
            : base(program)
        {
            this.program = program;
            this.eventCallback = eventCallback;
        }

        /// <summary>
        /// Gets the program
        /// </summary>
        internal DebugProgram Program
        {
            get { return program; }
        }

        /// <summary>
        /// Notify visual studio of a bound breakpoint.
        /// </summary>
        internal void OnBound(DebugPendingBreakpoint pendingBreakpoint)
        {
            // Notify VS
            eventCallback.Send(Program, new BreakpointBoundEvent(pendingBreakpoint));
        }

        /// <summary>
        /// Create a new location breakpoint.
        /// </summary>
        protected override DalvikLocationBreakpoint CreateLocationBreakpoint(DocumentPosition documentPosition, TypeEntry typeEntry, MethodEntry methodEntry, object data)
        {
            // Create breakpoint objects
            var pendingBreakpoint = (DebugPendingBreakpoint)data;
            var boundBreakpoint = new DebugBoundBreakpoint<DebugLocationBreakpoint>(pendingBreakpoint, this, enum_BP_TYPE.BPT_CODE, x => new DebugLocationBreakpoint(Jdwp.EventKind.BreakPoint, documentPosition, typeEntry, methodEntry, x));

            // Return breakpoint
            return boundBreakpoint.Breakpoint;
        }

        /// <summary>
        /// The given breakpoint has been clear by the VM.
        /// Now remove from the list.
        /// </summary>
        protected override void OnReset(DalvikBreakpoint breakpoint)
        {
            var bp = breakpoint as IDebugBreakpoint;
            // is this one of our breakpoints?
            if (bp != null)
            {
                // Notify VS
                bp.BoundBreakpoint.OnReset();
                eventCallback.Send(Program, new BreakpointUnboundEvent(bp.BoundBreakpoint));
                // Remove from list
                base.OnReset(breakpoint);
            }
        }

        /// <summary>
        /// Send the given event to VS.
        /// </summary>
        internal void Send(DebugThread thread, BreakpointEvent @event)
        {
            eventCallback.Send(thread, @event);
        }
    }
}
