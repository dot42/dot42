using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.DexOptimizer
{
    /// <summary>
    /// Replace branch instructions by shorter formats
    /// </summary>
    internal sealed class BranchOptimizer : IDexTransformation
    {
        public void Transform(MethodBody body)
        {
            // Prefix all aligned opcodes with a nop.
            // We'll decide later if we want to remove them.
            var instructions = body.Instructions;
            /*for (var i = 0; i < instructions.Count; i++ )
            {
                switch(instructions[i].OpCode)
                {
                    case OpCodes.Packed_switch:
                        instructions.Insert(i++, new Instruction(OpCodes.Nop));
                        break;
                }
            }*/

            body.UpdateInstructionOffsets();
            for (var i = 0; i < instructions.Count; i++)
            {
                var ins = instructions[i];
                switch (ins.OpCode)
                {
                    /*case OpCodes.Packed_switch:
                        if ((ins.Offset % 2) == 0)
                        {
                            instructions.RemoveAt(i - 1);
                        }
                        break;*/
                    case OpCodes.Goto:
                    case OpCodes.Goto_16:
                    case OpCodes.Goto_32:
                        var offset = (ins.Operand as Instruction).Offset - ins.Offset;
                        OpCodes opcode;
                        if (IsI8(offset))
                        {
                            opcode = OpCodes.Goto;
                        }
                        else if (IsI16(offset))
                        {
                            opcode = OpCodes.Goto_16;
                        } 
                        else
                        {
                            opcode = OpCodes.Goto_32;
                        }
                        if (ins.OpCode != opcode)
                        {
                            ins.OpCode = opcode;
                            body.UpdateInstructionOffsets();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Is the given a signed int that fits in 16 bits?
        /// </summary>
        private static bool IsI8(int value)
        {
            return (value >= sbyte.MinValue) && (value <= sbyte.MaxValue);
        }

        /// <summary>
        /// Is the given a signed int that fits in 16 bits?
        /// </summary>
        private static bool IsI16(int value)
        {
            return (value >= short.MinValue) && (value <= short.MaxValue);
        }
    }
}
