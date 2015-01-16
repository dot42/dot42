using System;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.DexLib.Instructions
{
    public sealed class SparseSwitchData : ISwitchData
    {
        private readonly SortedDictionary<int, Instruction> targets;

        public SparseSwitchData()
        {
            targets = new SortedDictionary<int, Instruction>();
        }

        public SortedDictionary<int, Instruction> Targets
        {
            get { return targets; }
        }

        /// <summary>
        /// Gets all target instructions.
        /// </summary>
        public IEnumerable<Instruction> GetTargets()
        {
            return Targets.Values;
        }

        /// <summary>
        /// Replace all occurrences of oldTarget with newTarget.
        /// </summary>
        public void ReplaceTarget(Instruction oldTarget, Instruction newTarget)
        {
            var keys = targets.Where(x => x.Value == oldTarget).Select(entry => entry.Key).ToList();
            foreach (var key in keys)
            {
                targets[key] = newTarget;
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, targets.Select(x => string.Format("{0} -> {1}", x.Key, x.Value)));
        }
    }
}