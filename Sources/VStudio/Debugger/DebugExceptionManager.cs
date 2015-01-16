using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Dot42.Mapping;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    public sealed class DebugExceptionManager : DalvikExceptionManager
    {
        private readonly DebugProgram program;
        private readonly EngineEventCallback eventCallback;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DebugExceptionManager(DebugProgram program, EngineEventCallback eventCallback)
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
        /// Process the given exception event.
        /// </summary>
        protected override void OnExceptionEvent(DebuggerLib.Events.Jdwp.Exception @event, DalvikThread thread)
        {
            base.OnExceptionEvent(@event, thread);

            // Get information about the exception
            var exceptionTypeId = Debugger.ObjectReference.ReferenceTypeAsync(@event.ExceptionObject.Object).Await(DalvikProcess.VmTimeout);
            var exceptionType = Process.ReferenceTypeManager[exceptionTypeId];

            // Prepare VS event
            var info = new EXCEPTION_INFO();
            info.bstrExceptionName = exceptionType.GetNameAsync().Await(DalvikProcess.VmTimeout);
            var caught = @event.IsCaught;
            info.dwState = caught
                               ? enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE
                               : enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT;
            program.GetName(out info.bstrProgramName);
            info.pProgram = program;
            info.guidType = GuidList.Guids.guidDot42DebuggerId;

            // Send VS event
            var vsEvent = new ExceptionEvent(info, info.bstrExceptionName, false);
            Send((DebugThread) thread, vsEvent);
        }

        /// <summary>
        /// Send the given event to VS.
        /// </summary>
        internal void Send(DebugThread thread, ExceptionEvent @event)
        {
            eventCallback.Send(thread, @event);
        }
    }
}
