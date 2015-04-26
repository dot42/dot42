using System.Linq;
using System.Threading;
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

        private int isProcessingExceptions;
        private Exception handlingEx;
        private DalvikThread handlingThread;
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
            var processingCount = Interlocked.Increment(ref isProcessingExceptions);
            if (processingCount > 1)
            {
                Interlocked.Decrement(ref isProcessingExceptions);
                DLog.Error(DContext.VSDebuggerEvent, "Exception ({0}) in debuggee while retrieving exception information. involved thread: {0}; exception.object={2}; original thread: {3}; original exception object: {4}", processingCount - 1, GetThreadId(thread), @event.ExceptionObject.Object, GetThreadId(handlingThread), handlingEx == null ? "(null)" : handlingEx.ExceptionObject.Object.ToString());
                Debugger.Process.ResumeAsync();
                // I have no idea why we have to resume twice, but if we dont, 
                // the debuggee will hang.
                Debugger.Process.ResumeAsync();
                cancelProcessing.Cancel();
                return;
            }

            handlingEx = @event;
            handlingThread = thread;
            cancelProcessing = new CancellationTokenSource();
            var cancelToken = cancelProcessing.Token;
            
            bool caught;
            string exceptionName = "(unknown)";
            string callStackTypeName="(unknown)";
            try
            {

                // Get information about the exception
                var exceptionTypeId = Debugger.ObjectReference.ReferenceTypeAsync(@event.ExceptionObject.Object)
                                            .Await(DalvikProcess.VmTimeout, cancelToken);
                var exceptionType = Process.ReferenceTypeManager[exceptionTypeId];

                exceptionName = exceptionType.GetNameAsync().Await(DalvikProcess.VmTimeout, cancelToken);
                caught = @event.IsCaught;

                if (!ShouldHandle(exceptionName, caught))
                {
                    SetBehavior(exceptionTypeId, ExceptionBehaviorMap[exceptionName])
                                    .Wait(DalvikProcess.VmTimeout, cancelToken);
                    Debugger.VirtualMachine.ResumeAsync();
                    return;
                }

                if (caught)
                {
                    callStackTypeName = thread.GetCallStack().First().GetReferenceType()
                                              .GetNameAsync().Await(DalvikProcess.VmTimeout, cancelToken);

                    // don't handle internal caught exceptions.
                    if (IsInternalName(callStackTypeName))
                    {
                        Debugger.VirtualMachine.ResumeAsync();
                        return;
                    }
                }
                
                base.OnExceptionEvent(@event, thread);

            }
            catch(System.Exception ex)
            {
                DLog.Error(DContext.VSDebuggerEvent, "Exception in debugger while processing exception: {0}. involved thread: {1}; exception.object={2}; exception type: {3}; callstack pos: {4}", ex.Message, GetThreadId(thread), @event.ExceptionObject.Object, exceptionName, callStackTypeName);
                Debugger.VirtualMachine.ResumeAsync();
                return;
            }
            finally
            {
                Interlocked.Decrement(ref isProcessingExceptions);
            }

           

            // Prepare VS event
            var info = new EXCEPTION_INFO();
            info.bstrExceptionName = exceptionName;
            info.dwState = caught
                ? enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE
                : enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT;
            program.GetName(out info.bstrProgramName);
            info.pProgram = program;
            info.guidType = GuidList.Guids.guidDot42DebuggerId;

            string description = info.bstrExceptionName;
            
            if (caught)
                description += " (first chance, caught by debuggee)";
            else
                description += " (not caught by debugee)";

            if (thread == null)
            {
                DLog.Error(DContext.VSDebuggerEvent, "Exception without a thread: {0}; original thread id: {1}.", exceptionName, @event.ThreadId);
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

        private bool ShouldHandle(string exceptionName, bool caught)
        {
            // don't handle internal caught exceptions.
            if (IsInternalName(exceptionName) && caught)
                return false;

            var behavior = ExceptionBehaviorMap[exceptionName];
            if ((caught && behavior.StopOnThrow) || (!caught && behavior.StopUncaught))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsInternalName(string name)
        {
            return name.StartsWith("libcore.");
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
