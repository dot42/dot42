using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib.Bytecode
{
    /// <summary>
    /// Data payload for a TABLESWITCH instruction.
    /// </summary>
    public class TableSwitchData : IResolveable
    {
        private readonly BranchOffset defaultByte;
        private Instruction defaultIns;
        public readonly int LowByte;
        public readonly int HighByte;
        private readonly BranchOffset[] offsets;
        private Instruction[] targets;

        internal TableSwitchData(BranchOffset defaultByte, int lowByte, int highByte, BranchOffset[] offsets)
        {
            this.defaultByte = defaultByte;
            LowByte = lowByte;
            HighByte = highByte;
            this.offsets = offsets;
        }

        public Instruction DefaultTarget { get { return defaultIns; } }
        public Instruction[] Targets { get { return targets; } }

        object IResolveable.Resolve(List<Instruction> instructions, Instruction owner)
        {
            defaultIns = defaultByte.Resolve(instructions, owner);
            targets = offsets.Select(x => x.Resolve(instructions, owner)).ToArray();
            return this;
        }
    }
}
