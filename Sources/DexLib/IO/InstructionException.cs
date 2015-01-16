using System;
using Dot42.DexLib.Instructions;

namespace Dot42.DexLib.IO
{
    public class InstructionException : MalformedException
    {
        public InstructionException(Instruction instruction, String message)
            : base(message)
        {
            Instruction = instruction;
        }

        public Instruction Instruction { get; set; }
    }
}