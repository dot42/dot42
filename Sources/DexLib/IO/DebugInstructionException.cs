using System;
using Dot42.DexLib.Instructions;

namespace Dot42.DexLib.IO
{
    public class DebugInstructionException : MalformedException
    {
        public DebugInstructionException(DebugInstruction instruction, String message)
            : base(message)
        {
            Instruction = instruction;
        }

        public DebugInstruction Instruction { get; set; }
    }
}