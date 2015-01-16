using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Wrapper around IDebugEventCallback2
    /// </summary>
    internal sealed class EngineEventCallback
    {
        private readonly DebugEngine engine;
        private readonly IDebugEventCallback2 callback;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EngineEventCallback(DebugEngine engine, IDebugEventCallback2 callback)
        {
            this.engine = engine;
            this.callback = callback;
        }

        /// <summary>
        /// Send an event from the given sender.
        /// </summary>
        public void Send(BaseEvent @event)
        {
            Send(null, null, null, @event);
        }

        /// <summary>
        /// Send an event from the given sender.
        /// </summary>
        public void Send(IDebugProcess2 sender, BaseEvent @event)
        {
            Send(sender, null, null, @event);
        }

        /// <summary>
        /// Send an event from the given sender.
        /// </summary>
        public void Send(DebugProgram sender, BaseEvent @event)
        {
            Send(sender.Process, sender, null, @event);
        }

        /// <summary>
        /// Send an event from the given sender.
        /// </summary>
        public void Send(DebugThread sender, BaseEvent @event)
        {
            var program = sender.Program;
            Send(program.Process, program, sender, @event);
        }

        /// <summary>
        /// Perform actual send
        /// </summary>
        private void Send(IDebugProcess2 process, IDebugProgram2 program, IDebugThread2 thread, BaseEvent @event)
        {
            var guid = @event.IID;
            DLog.Debug(DContext.VSDebuggerEvent, "DebugEngine Event {0} {1}", @event.GetType().Name, guid);
            var rc = callback.Event(engine, process, program, thread, @event, ref guid, (uint)@event.Attributes);            
            if (!ErrorHandler.Succeeded(rc))
            {
                DLog.Error(DContext.VSDebuggerEvent, "DebugEngine Event failed {0}", rc);
            }
        }
    }
}
