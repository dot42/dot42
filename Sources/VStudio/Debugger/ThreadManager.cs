using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Model;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Holds a mapping between VM threads and VS IDebugThread2 instances.
    /// </summary>
    internal class ThreadManager : DalvikThreadManager
    {
        private readonly DebugProgram program;
        private readonly EngineEventCallback eventCallback;
        private int lastTid = 1;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ThreadManager(DebugProgram program, EngineEventCallback eventCallback)
            : base(program)
        {
            this.program = program;
            this.eventCallback = eventCallback;
        }

        /// <summary>
        /// Gets all known threads.
        /// This method is thread safe.
        /// </summary>
        public new IEnumerable<DebugThread> Threads
        {
            get { return base.Threads.Cast<DebugThread>(); }
        }

        /// <summary>
        /// Refresh the list of threads.
        /// </summary>
        public Task RefreshAsync()
        {
            return RefreshThreadsAsync();
        }
        
        /// <summary>
        /// Gets the main thread.
        /// Returns null if not found.
        /// </summary>
        public new DebugThread MainThread()
        {
            return (DebugThread) base.MainThread();
        }

        /// <summary>
        /// Create an instance representing the given reference type.
        /// </summary>
        protected override DalvikThread CreateThread(DebuggerLib.ThreadId id)
        {
            return new DebugThread(program, eventCallback, this, id, lastTid++);
        }

        /// <summary>
        /// The given thread has been created.
        /// </summary>
        protected override void OnThreadCreated(DalvikThread thread)
        {
            var dthread = (DebugThread) thread;
            base.OnThreadCreated(dthread);
            eventCallback.Send(dthread, new ThreadCreateEvent());
        }

        /// <summary>
        /// The given thread has ended.
        /// </summary>
        protected override void OnThreadEnd(DalvikThread thread)
        {
            var dthread = (DebugThread)thread;
            base.OnThreadEnd(dthread);
            eventCallback.Send(dthread, new ThreadDestroyEvent());
        }
    }
}
