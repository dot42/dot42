using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.Utility;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain thread information
    /// </summary>
    public class DalvikThreadManager : DalvikProcessChild
    {
        private readonly Dictionary<ThreadId, DalvikThread> threads = new Dictionary<ThreadId, DalvikThread>();
        private readonly List<DalvikThread> list = new List<DalvikThread>();
        private readonly object threadsLock = new object();

        /// <summary>
        /// Default ctor
        /// </summary>
        protected internal DalvikThreadManager(DalvikProcess process)
            : base(process)
        {
        }

        /// <summary>
        /// Gets all known threads.
        /// This method is thread safe.
        /// </summary>
        public IEnumerable<DalvikThread> Threads
        {
            get
            {
                lock (threadsLock)
                {
                    return new List<DalvikThread>(list);
                }
            }
        }

        /// <summary>
        /// Gets the main thread.
        /// Returns null if not found.
        /// </summary>
        public DalvikThread MainThread()
        {
            foreach (var thread in Threads) // Use property here because it has made a thread safe copy
            {
                var name = thread.GetNameAsync().Await(DalvikProcess.VmTimeout);
                if (name.IndexOf("main", StringComparison.OrdinalIgnoreCase) >= 0)
                    return thread;
            }
            // Not found, just return the first
            return list.FirstOrDefault();
        }

        /// <summary>
        /// Try to get a thread by it's id.
        /// </summary>
        public bool TryGet(ThreadId id, out DalvikThread thread)
        {
            lock (threadsLock)
            {
                return threads.TryGetValue(id, out thread);
            }            
        }

        /// <summary>
        /// Refresh the thread list and notify all threads that's we've justed suspended the process.
        /// </summary>
        /// <param name="reason">The reason the VM is suspended</param>
        /// <param name="thread">The thread involved in the suspend. This can be null depending on the reason.</param>
        internal void OnProcessSuspended(SuspendReason reason, DalvikThread thread)
        {
            // Ask all threads to cleanup
            foreach (var t in Threads)
            {
            	t.OnProcessSuspended(reason, (t == thread));
            }
            // Refresh threads
            RefreshThreadsAsync().Wait();
        }

        /// <summary>
        /// Refresh the list of threads
        /// </summary>
        protected Task RefreshThreadsAsync()
        {
            return Debugger.VirtualMachine.AllThreadsAsync().ContinueWith(ProcessAllThreads);            
        }

        /// <summary>
        /// Process the result of an AllThreads request.
        /// </summary>
        private void ProcessAllThreads(Task<List<ThreadId>> task)
        {
            if (!task.CompletedOk())
            {
                DLog.Error(DContext.DebuggerLibModel, "LoadAllThreads failed", task.Exception);
                task.ForwardException();
                return;
            }

            List<DalvikThread> created = null;
            List<DalvikThread> removed = null;
            lock (threadsLock)
            {
                // Create missing threads
                foreach (var id in task.Result.Where(x => !threads.ContainsKey(x)))
                {
                    // Create and record
                    var thread = CreateThread(id);
                    threads[id] = thread;
                    list.Add(thread);

                    created = created ?? new List<DalvikThread>();
                    created.Add(thread);
                }

                // Remove obsolete threads
                var toRemove = threads.Keys.Where(x => !task.Result.Contains(x)).ToList();
                foreach (var id in toRemove)
                {
                    var thread = threads[id];
                    threads.Remove(id);
                    list.Remove(thread);
                    removed = removed ?? new List<DalvikThread>();
                    removed.Add(thread);
                }
            }

            if (created != null) created.ForEach(OnThreadCreated);
            if (removed != null) removed.ForEach(OnThreadEnd);
        }

        /// <summary>
        /// The given thread has been created.
        /// </summary>
        protected virtual void OnThreadCreated(DalvikThread thread)
        {            
        }

        /// <summary>
        /// The given thread has ended.
        /// </summary>
        protected virtual void OnThreadEnd(DalvikThread thread)
        {
        }

        /// <summary>
        /// Create an instance representing the given reference type.
        /// </summary>
        protected virtual DalvikThread CreateThread(ThreadId id)
        {
            return new DalvikThread(id, this);
        }
    }
}
