using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib.Bytecode
{
    internal class BranchOffset : IResolveable
    {
        private readonly int offset;

        /// <summary>
        /// Default ctor
        /// </summary>
        public BranchOffset(int offset)
        {
            this.offset = offset;
        }

        /// <summary>
        /// Resolve this offset into an instruction.
        /// </summary>
        public Instruction Resolve(List<Instruction> instructions, Instruction owner)
        {
            var absOffset = owner.Offset + offset;
            return instructions.First(x => x.Offset == absOffset);
        }

        /// <summary>
        /// Resolve this offset into an instruction.
        /// </summary>
        object IResolveable.Resolve(List<Instruction> instructions, Instruction owner)
        {
            return Resolve(instructions, owner);
        }
    }
}
