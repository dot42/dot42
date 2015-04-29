using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Location specific breakpoint.
    /// </summary>
    internal sealed class DebugLocationBreakpoint : DalvikLocationBreakpoint, IDebugBreakpoint
    {
        private readonly DebugBoundBreakpoint<DebugLocationBreakpoint> boundBreakpoint;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugLocationBreakpoint(Jdwp.EventKind eventKind, Document document, DocumentPosition documentPosition, TypeEntry typeEntry, MethodEntry methodEntry, DebugBoundBreakpoint<DebugLocationBreakpoint> boundBreakpoint)
            : base(eventKind, document, documentPosition, typeEntry, methodEntry)
        {
            this.boundBreakpoint = boundBreakpoint;
        }

        public DebugLocationBreakpoint(Location location, DebugBoundBreakpoint<DebugLocationBreakpoint> boundBreakpoint, DocumentLocation documentLocation = null)
            : base(location, documentLocation)
        {
            this.boundBreakpoint = boundBreakpoint;
        }

        /// <summary>
        /// This breakpoint is bound to it's target
        /// </summary>
        protected override void OnBound(int requestId, DalvikBreakpointManager breakpointManager)
        {
            base.OnBound(requestId, breakpointManager);
            // Notify VS
            boundBreakpoint.OnBound(this);
        }

        /// <summary>
        /// This breakpoint is reached.
        /// </summary>
        protected override void OnTrigger(DebuggerLib.Events.Jdwp.Breakpoint @event)
        {
            // Notify VS
            boundBreakpoint.OnTrigger(@event);
        }

        /// <summary>
        /// Gets the containing bound breakpoint.
        /// </summary>
        public IDebugBoundBreakpoint BoundBreakpoint { get { return boundBreakpoint; } }
    }
}
