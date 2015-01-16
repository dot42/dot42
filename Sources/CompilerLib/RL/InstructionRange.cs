using System.Collections.Generic;
using System.Diagnostics;

namespace Dot42.CompilerLib.RL
{
    [DebuggerDisplay("{First.Index}-{Last.Index}")]
    public class InstructionRange : IInstructionRange
    {
        private readonly Instruction first;
        private readonly Instruction last;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InstructionRange(Instruction first, Instruction last)
        {
            this.first = first;
            this.last = last;
        }

        /// <summary>
        /// First instruction in the range.
        /// </summary>
        public Instruction First { get { return first; } }

        /// <summary>
        /// Last (inclusive) instruction in the range.
        /// </summary>
        public Instruction Last { get { return last; } }

        /// <summary>
        /// Get all instructions in the range.
        /// </summary>
        public IEnumerable<Instruction> Instructions
        {
            get
            {
                var i = first.Index;
                var list = first.Parent;
                while (true)
                {
                    var current = list[i++];
                    yield return current;
                    if (current == last)
                        yield break;                    
                }
            }
        }
    }
}
