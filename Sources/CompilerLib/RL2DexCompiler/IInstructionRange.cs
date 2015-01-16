using System.Collections.Generic;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    public interface IInstructionRange
    {
        /// <summary>
        /// First instruction in the range.
        /// </summary>
        Instruction First { get; }

        /// <summary>
        /// Last (inclusive) instruction in the range.
        /// </summary>
        Instruction Last { get; }

        /// <summary>
        /// Get all instructions in the range.
        /// </summary>
        IEnumerable<Instruction> Instructions { get; }
    }
}
