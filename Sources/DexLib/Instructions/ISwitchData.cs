using System.Collections.Generic;

namespace Dot42.DexLib.Instructions
{
    public interface ISwitchData
    {
        /// <summary>
        /// Gets all target instructions.
        /// </summary>
        IEnumerable<Instruction> GetTargets();

        /// <summary>
        /// Replace all occurrences of oldTarget with newTarget.
        /// </summary>
        void ReplaceTarget(Instruction oldTarget, Instruction newTarget);
    }
}
