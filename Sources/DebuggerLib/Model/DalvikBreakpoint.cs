namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Represents a single break point in the code.
    /// </summary>
    public abstract class DalvikBreakpoint
    {
        private readonly Jdwp.EventKind eventKind;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected DalvikBreakpoint(Jdwp.EventKind eventKind)
        {
            this.eventKind = eventKind;
        }

        /// <summary>
        /// Event request ID returned from JDWP Set request.
        /// </summary>
        public int RequestId { get; private set; }

        /// <summary>
        /// Is this breakpoint bound to the VM?
        /// </summary>
        public bool IsBound { get { return (RequestId > 0); } }

        /// <summary>
        /// Kind of event
        /// </summary>
        public Jdwp.EventKind EventKind
        {
            get { return eventKind; }
        }

        /// <summary>
        /// This breakpoint is bound to it's target
        /// </summary>
        protected internal virtual void OnBound(int requestId, DalvikBreakpointManager breakpointManager)
        {
            RequestId = requestId;
        }

        /// <summary>
        /// Try to bind this breakpoint to an actual breakpoint in the VM.
        /// </summary>
        internal abstract void TryBind(DalvikProcess process);

        /// <summary>
        /// This breakpoint is reached.
        /// </summary>
        protected internal virtual void OnTrigger(Events.Jdwp.Breakpoint @event)
        {
            // Override me
        }
    }
}
