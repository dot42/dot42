using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain thread information
    /// </summary>
    public class DalvikThread
    {
        public readonly ThreadId Id;
        private readonly DalvikThreadManager manager;
        private List<DalvikStackFrame> callStack;
        private Jdwp.ThreadStatus? status;
        private bool? suspended;
        private int? suspendCount;
        private string name;

        public DalvikThread(ThreadId id, DalvikThreadManager manager)
        {
            Id = id;
            this.manager = manager;
        }

        /// <summary>
        /// Exception (if any)
        /// </summary>
        public TaggedObjectId CurrentException { get; set; }

        /// <summary>
        /// Gets all stackframes of the callstack of this thread.
        /// </summary>
        public IEnumerable<DalvikStackFrame> GetCallStack()
        {
            if (callStack == null)
            {
                callStack = Debugger.ThreadReference.FramesAsync(Id).Await(DalvikProcess.VmTimeout).Select(t => CreateStackFrame(t.Item1, t.Item2)).ToList();
                if ((CurrentException != null) && (callStack.Count > 0))
                {
                    // Set exception in top of call stack
                    callStack[0].Exception = CurrentException;
                }
            }
            return callStack;
        }

        /// <summary>
        /// Gets the status of this thread.
        /// </summary>
        public Task<Jdwp.ThreadStatus> GetStatusAsync()
        {
            return status.HasValue ? status.Value.AsTask() : RefreshStatusAsync().ContinueWith(x => x.Result.ThreadStatus);
        }

        /// <summary>
        /// Is this thread suspended?
        /// </summary>
        public Task<bool> GetSuspendedAsync()
        {
            return suspended.HasValue ? suspended.Value.AsTask() : RefreshStatusAsync().ContinueWith(x => suspended.Value);
        }

        /// <summary>
        /// Get the number of times this thread is suspended without being resumed?
        /// </summary>
        public Task<int> GetSuspendCountAsync()
        {
            return suspendCount.HasValue ? suspendCount.Value.AsTask() : RefreshSuspendCountAsync();
        }

        /// <summary>
        /// Gets the name of this thread.
        /// </summary>
        public Task<string> GetNameAsync()
        {
            return name != null ? name.AsTask() : RefreshNameAsync();
        }

        /// <summary>
        /// Refresh the status of this thread.
        /// </summary>
        protected Task<ThreadStatusInfo> RefreshStatusAsync()
        {
            return Debugger.ThreadReference.StatusAsync(Id).SaveAndReturn(x => {
                status = x.ThreadStatus;
                suspended = x.SuspendStatus == Jdwp.SuspendStatus.Suspended;
            });
        }

        /// <summary>
        /// Refresh the suspend count of this thread.
        /// </summary>
        protected Task<int> RefreshSuspendCountAsync()
        {
            return Debugger.ThreadReference.SuspendCountAsync(Id).SaveAndReturn(x => suspendCount = x);
        }

        /// <summary>
        /// Refresh the name of this thread.
        /// </summary>
        protected Task<string> RefreshNameAsync()
        {
            return Debugger.ThreadReference.NameAsync(Id).SaveAndReturn(x => name = x);
        }

        /// <summary>
        /// Create a stack frame for the given parameters.
        /// </summary>
        protected virtual DalvikStackFrame CreateStackFrame(FrameId frameId, Location location)
        {
            return new DalvikStackFrame(frameId, location, this);
        }

        /// <summary>
        /// Provide access to the containing manager.
        /// </summary>
        protected internal DalvikThreadManager Manager { get { return manager; } }

        /// <summary>
        /// Provide access to the low level debugger.
        /// </summary>
        protected Debugger Debugger { get { return manager.Debugger; } }

        /// <summary>
        /// We've justed suspended the process.
        /// Refresh call stack.
        /// </summary>
        protected internal virtual void OnProcessSuspended(SuspendReason reason, bool isCurrentThread)
        {
            callStack = null;
            status = null;
            suspended = null;
            name = null;
            suspendCount = null;
            CurrentException = null;
        }
    }
}
