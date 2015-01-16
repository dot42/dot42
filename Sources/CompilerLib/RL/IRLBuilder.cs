using System.Collections.Generic;

namespace Dot42.CompilerLib.RL
{
    internal interface IRLBuilder
    {
        /// <summary>
        /// Create and add an instruction.
        /// </summary>
        Instruction Add(ISourceLocation sequencePoint, RCode opcode, object operand, IEnumerable<Register> registers);
    }
}
