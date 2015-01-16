using System.Collections.Generic;
using System.Linq;

namespace Dot42.DexLib.Instructions
{
    public sealed class PackedSwitchData : ISwitchData
    {
        private readonly List<Instruction> targets;

        public PackedSwitchData()
        {
            targets = new List<Instruction>();
        }

        public PackedSwitchData(IEnumerable<Instruction> targets)
        {
            this.targets = targets.ToList();
        }

        public int FirstKey { get; set; }
        public List<Instruction> Targets
        {
            get { return targets; }
        }

        /// <summary>
        /// Gets all target instructions.
        /// </summary>
        public IEnumerable<Instruction> GetTargets()
        {
            return targets;
        }

        /// <summary>
        /// Replace all occurrences of oldTarget with newTarget.
        /// </summary>
        public void ReplaceTarget(Instruction oldTarget, Instruction newTarget)
        {
            var count = targets.Count;
            for (var i = 0; i < count; i++)
            {
                if (targets[i] == oldTarget)
                {
                    targets[i] = newTarget;
                }
            }
        }
    }
}