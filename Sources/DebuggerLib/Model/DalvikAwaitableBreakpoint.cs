using System.Collections.Concurrent;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.Mapping;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// represents a temporary breakpoint, that can be awaited upon
    /// </summary>
    public class DalvikAwaitableBreakpoint : DalvikLocationBreakpoint
    {
        private readonly ConcurrentQueue<TaskCompletionSource<object>> _waiters = new ConcurrentQueue<TaskCompletionSource<object>>();

        public DalvikAwaitableBreakpoint(Location location) : base(location)
        {
        }

        public Task WaitUntilHit()
        {
            var task = new TaskCompletionSource<object>();
            _waiters.Enqueue(task);
            return task.Task;
        }

        protected internal override void OnTrigger(Breakpoint @event)
        {
            TaskCompletionSource<object> task;
            while(_waiters.TryDequeue(out task))
                task.SetResult(null);
        }
    }
}
