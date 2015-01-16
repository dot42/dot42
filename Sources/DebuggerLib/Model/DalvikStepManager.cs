using System.Collections.Generic;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Handle stepping
    /// </summary>
    internal class DalvikStepManager
    {
        private readonly Dictionary<int, DalvikStep> steps = new Dictionary<int, DalvikStep>();
        private readonly object dataLock = new object();

        /// <summary>
        /// Add the given step to the list.
        /// </summary>
        public void Add(DalvikStep step)
        {
            lock (dataLock)
            {
                steps.Add(step.RequestId, step);
            }
        }

        /// <summary>
        /// Gets a step by it's request id.
        /// Remove the step also.
        /// </summary>
        public DalvikStep GetAndRemove(int requestId)
        {
            lock (dataLock)
            {
                DalvikStep result;
                if (steps.TryGetValue(requestId, out result))
                {
                    steps.Remove(requestId);
                }
                else
                {
                    result = null;
                }
                return result;
            }
        }
    }
}
