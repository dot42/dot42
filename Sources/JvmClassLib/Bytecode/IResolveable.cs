using System.Collections.Generic;

namespace Dot42.JvmClassLib.Bytecode
{
    internal interface IResolveable
    {
        /// <summary>
        /// Resolve this offset into an instruction.
        /// </summary>
        object Resolve(List<Instruction> instructions, Instruction owner);
    }
}
