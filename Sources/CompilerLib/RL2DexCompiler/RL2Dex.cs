using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.CompilerLib.RL2DexCompiler.Extensions;
using Dot42.DexLib.Instructions;
using Instruction = Dot42.DexLib.Instructions.Instruction;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    /// <summary>
    /// Convert RL instructions to Dex instructions
    /// </summary>
    internal static class RL2Dex
    {
        /// <summary>
        /// Convert the given instruction into 1 or more dex instructions.
        /// </summary>
        internal static IEnumerable<Instruction> Convert(RL.Instruction source, RegisterMapper regMapper)
        {
            var dexIns = new Instruction(source.Code.ToDex(), source.Operand) { SequencePoint = source.SequencePoint };
            var dexRegisters = dexIns.Registers;
            dexRegisters.AddRange(source.Registers.Select(x => regMapper[x]));
            if (!AllRegistersFit(dexIns) || dexIns.RequiresInvokeRange())
            {
                // At least 1 register does not fit.
                // Insert a NOP first so we do not have to re-route when we insert spilling code.
                yield return new Instruction(OpCodes.Nop);
            }
            yield return dexIns;
        }

        /// <summary>
        /// Do all registers of the given instruction fit in the size that is available for them?
        /// </summary>
        private static bool AllRegistersFit(Instruction ins)
        {
            var registers = ins.Registers;
            var count = registers.Count;
            if (count == 0)
                return true;
            var info = OpCodeInfo.Get(ins.OpCode);
            for (var i = 0; i < count; i++)
            {
                var size = info.GetUsage(i) & RegisterFlags.SizeMask;
                switch (size)
                {
                    case RegisterFlags.Bits4:
                        if (!registers[i].IsBits4)
                            return false;
                        break;
                    case RegisterFlags.Bits8:
                        if (!registers[i].IsBits8)
                            return false;
                        break;
                }
            }
            return true;
        }
    }
}
