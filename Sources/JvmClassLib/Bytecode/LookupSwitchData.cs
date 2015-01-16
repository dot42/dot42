using System.Collections.Generic;
using System.Text;

namespace Dot42.JvmClassLib.Bytecode
{
    /// <summary>
    /// Data payload for a LOOKUPSWITCH instruction.
    /// </summary>
    public class LookupSwitchData : IResolveable
    {
        private readonly BranchOffset defaultByte;
        public readonly Pair[] Pairs;
        private Instruction defaultIns;

        internal LookupSwitchData(BranchOffset defaultByte, Pair[] pairs)
        {
            this.defaultByte = defaultByte;
            Pairs = pairs;
        }

        public Instruction DefaultTarget
        {
            get { return defaultIns; }
        }

        object IResolveable.Resolve(List<Instruction> instructions, Instruction owner)
        {
            defaultIns = defaultByte.Resolve(instructions, owner);
            foreach (var pair in Pairs)
            {
                pair.Resolve(instructions, owner);
            }
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Default -> {0:X4}", DefaultTarget.Offset);
            sb.AppendLine();
            foreach (var p in Pairs)
            {
                sb.AppendFormat("{0} -> {1:X4}", p.Match, p.Target.Offset);
                sb.AppendLine();                
            }
            return sb.ToString();
        }

        public class Pair
        {
            public readonly int Match;
            private readonly BranchOffset offset;
            private Instruction target;

            internal Pair(int match, BranchOffset offset)
            {
                Match = match;
                this.offset = offset;
            }

            internal void Resolve(List<Instruction> instructions, Instruction owner)
            {
                target = offset.Resolve(instructions, owner);
            }

            public Instruction Target
            {
                get { return target; }
            }
        }
    }
}
