using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.DebuggerLib.Model;
using Dot42.Utility;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    public sealed class DebugExceptionManager : DalvikExceptionManager
    {
        private readonly DebugProgram program;
        private readonly EngineEventCallback eventCallback;

        private Exception processing;
        private CancellationTokenSource cancelProcessing;

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
        protected override void OnExceptionEvent(Exception @event, DalvikThread thread)
        {
            var prev = Interlocked.CompareExchange(ref processing, @event, null);
            if (prev != null)
            {
                if (@event.ExceptionObject.Equals(prev.ExceptionObject) && @event.ThreadId.Equals(prev.ThreadId))
                {
                    // the same exception is reported multiple times. just ignore.
                    Debugger.VirtualMachine.ResumeAsync();
                    return;
                }

                DLog.Error(DContext.VSDebuggerMessage, 
                    "Multiple exceptions in debuggee or exceptions while retrieving exception information. "
                   +"Current Exception/Thread: {0}/{1}; previous Exception/Thread: {2}/{3} ", 
                   @event.ThreadId, @event.ExceptionObject, prev.ThreadId, prev.ExceptionObject);

                Debugger.VirtualMachine.ResumeAsync();
                // I have no idea why we have to resume twice, but if we dont, the debuggee will hang.
                Debugger.Process.ResumeAsync();

                if(cancelProcessing != null)
                    cancelProcessing.Cancel();
            }

            cancelProcessing = new CancellationTokenSource();
            var cancelToken = cancelProcessing.Token;
            bool wasThreadNull = thread == null;
            
            bool caught;
            string exceptionName = "(unknown)";
            string callStackTypeName="(unknown)";
            string exceptionMessage = null;

            try
            {
                // Get information about the exception
                var exceptionTypeId = Debugger.ObjectReference.ReferenceTypeAsync(@event.ExceptionObject.Object)
                                                              .Await(DalvikProcess.VmTimeout, cancelToken);
                var exceptionType = Process.ReferenceTypeManager[exceptionTypeId];

                exceptionName = exceptionType.GetNameAsync()
                                             .Await(DalvikProcess.VmTimeout, cancelToken);
                caught = @event.IsCaught;

                if (!ShouldHandle(exceptionName, caught))
                {
                    Debugger.VirtualMachine.ResumeAsync();
                    return;
                }

                if (caught && thread != null)
                {
                    // don't handle excluded locations.
                    // check the first two stackframes.
                    foreach (var frame in thread.GetCallStack().Take(2))
                    {
                        callStackTypeName = frame.GetReferenceType().GetNameAsync()
                                                                    .Await(DalvikProcess.VmTimeout, cancelToken);

                        if (CaughtExceptionLocationExcludePattern.IsMatch(callStackTypeName))
                        {
                            Debugger.VirtualMachine.ResumeAsync();
                            return;
                        }
                    }
                }

                if (wasThreadNull)
                    thread = Debugger.Process.ThreadManager.Threads.First();
                
                base.OnExceptionEvent(@event, thread);

                exceptionMessage = GetExceptionMessageAsync(@event.ExceptionObject).Await(DalvikProcess.VmTimeout);
            }
            catch(System.Exception ex)
            {
                DLog.Error(DContext.VSDebuggerMessage, "Exception in debugger while processing exception: {0}. involved thread: {1}; exception.object={2}; exception type: {3}; callstack pos: {4}", ex.Message, GetThreadId(thread), @event.ExceptionObject.Object, exceptionName, callStackTypeName);
                Debugger.VirtualMachine.ResumeAsync();
                return;
            }
            finally
            {
                Interlocked.Exchange(ref processing, null);
            }

            // Prepare VS event
            var info = new EXCEPTION_INFO();
            info.bstrExceptionName = exceptionName;
            info.dwState = caught ? enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE
                                  : enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT;
            program.GetName(out info.bstrProgramName);
            info.pProgram = program;
            info.guidType = GuidList.Guids.guidDot42DebuggerId;

            string description = info.bstrExceptionName;

            if (exceptionMessage != null)
                description += ": \"" + exceptionMessage + "\"";
            
            if (caught)
                description += "\n(first chance, caught by debuggee)";
            else
                description += "\n(not caught by debugee)";

            if (thread == null)
            {
                DLog.Warning(DContext.VSDebuggerEvent, "Exception without a thread: {0}. Original thread id: {1}.", exceptionName, @event.ThreadId);
                description += "\n  The exceptions thread has already died, the VS call stack window has no meaning. The exception was raised on thread "+ @event.ThreadId;
            }

            // Send VS event
            var vsEvent = new ExceptionEvent(info, description, false);
            Send((DebugThread)thread, vsEvent);
        }

        private static string GetThreadId(DalvikThread thread)
        {
            return thread == null ? "(none)" : thread.Id.ToString();
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
