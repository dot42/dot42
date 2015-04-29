using System.Collections.Concurrent;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Handle stepping
    /// </summary>
    internal class DalvikStepManager
    {
        private readonly ConcurrentDictionary<int, DalvikStep> steps = new ConcurrentDictionary<int, DalvikStep>();

        /// <summary>
        /// Add the given step to the list.
        /// </summary>
        public void Add(DalvikStep step)
        {
            steps.TryAdd(step.RequestId, step);
        }

        /// <summary>
        /// Gets a step by it's request id.
        /// Remove the step also.
        /// </summary>
        public DalvikStep GetAndRemove(int requestId)
        {
            DalvikStep result;
            if (steps.TryRemove(requestId, out result))
                return result;
            return null;
        }
    }
}
