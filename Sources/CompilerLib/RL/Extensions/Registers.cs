using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.DexLib.Instructions;
using Dot42.Utility;

namespace Dot42.CompilerLib.RL.Extensions
{
    partial class RLExtensions
    {
        /// <summary>
        /// Does any of the instructions in the given block use the given register?
        /// </summary>
        public static bool Uses(this BasicBlock block, Register r)
        {
            return block.Instructions.Any(x => x.Uses(r));
        }

        /// <summary>
        /// Does any of the instructions in the given range use the given register?
        /// </summary>
        public static bool Uses(this IInstructionRange range, Register r)
        {
            return range.Instructions.Any(x => x.Uses(r));
        }

        /// <summary>
        /// Gets the first instruction that uses the given register?
        /// </summary>
        public static Instruction GetFirstUse(this Register r, IEnumerable<Instruction> instructions)
        {
            return instructions.FirstOrDefault(x => x.Uses(r));
        }

        /// <summary>
        /// Gets the last instruction that uses the given register?
        /// </summary>
        public static Instruction GetLastUse(this Register r, List<Instruction> instructions)
        {
            return instructions.LastOrDefault(x => x.Uses(r));
        }

        /// <summary>
        /// Gets the all instructions that uses the given register.
        /// </summary>
        public static IEnumerable<Instruction> GetUses(this Register r, IEnumerable<Instruction> instructions)
        {
            return instructions.Where(x => x.Uses(r));
        }

        /// <summary>
        /// Gets the instruction range where the given register is used?
        /// </summary>
        public static IInstructionRange GetUsageRange(this Register r, IEnumerable<Instruction> instructions)
        {
            var list = instructions.ToList();
            var first = r.GetFirstUse(list);
            if (first == null)
                return null;
            var last = r.GetLastUse(list);
            return new InstructionRange(first, last);
        }

        /// <summary>
        /// Is the given register being assigned in the given instruction?
        /// </summary>
        public static bool IsDestinationIn(this Register r, Instruction i)
        {
            var index = i.Registers.IndexOf(r);
            return (index >= 0) && i.IsDestinationRegister(index);
        }

        /// <summary>
        /// Replace all references to oldRegister with newRegister in the given instruction set.
        /// </summary>
        public static void ReplaceRegisterWith(this IEnumerable<Instruction> instructions, Register oldRegister, Register newRegister, MethodBody body)
        {
            var oldRegister2 = (oldRegister.Type == RType.Wide) ? body.GetNext(oldRegister) : null;
            var newRegister2 = (newRegister.Type == RType.Wide) ? body.GetNext(newRegister) : null;

            if (oldRegister.KeepWith != newRegister.KeepWith)
                throw new ArgumentException("New register has different keep-with value");

            foreach (var ins in instructions)
            {
                ins.ReplaceRegisterWith(oldRegister, newRegister);
                if (oldRegister2 != null)
                    ins.ReplaceRegisterWith(oldRegister2, newRegister2);
            }
        }
    }
}

