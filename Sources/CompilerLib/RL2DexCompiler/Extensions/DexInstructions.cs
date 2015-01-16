using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler.Extensions
{
    partial class ILCompilerExtensions
    {
        /// <summary>
        /// Get the instruction direct before the given instruction.
        /// </summary>
        internal static Instruction GetPrevious(this Instruction instruction, List<Instruction> instructions)
        {
            for (var index = 0; index < instructions.Count; index++)
            {
                if (instructions[index] == instruction)
                {
                    if (index == 0)
                        throw new ArgumentException("Instruction is first instruction in list");
                    return instructions[index - 1];
                }
            }
            throw new ArgumentException("Instruction is not part of given list");
        }

        /// <summary>
        /// Get the instruction direct after the given instruction.
        /// </summary>
        internal static bool TryGetNext(this Instruction instruction, List<Instruction> instructions, out Instruction next)
        {
            next = null;
            for (var index = 0; index < instructions.Count; index++)
            {
                if (instructions[index] == instruction)
                {
                    if (index == instructions.Count - 1)
                        return false;
                    next = instructions[index + 1];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is an invoke range required for this instruction?
        /// </summary>
        public static bool RequiresInvokeRange(this Instruction ins)
        {
            return (ins.Registers.Count > 5) || ins.Registers.Any(x => !x.IsBits4);
        }

        /// <summary>
        /// Is the given register used as source in the given instruction?
        /// </summary>
        public static bool IsSourceIn(this Register register, Instruction ins)
        {
            var registers = ins.Registers;
            var count = registers.Count;
            if (count == 0)
                return false;
            var info = OpCodeInfo.Get(ins.OpCode);
            for (var i = 0; i < count; i++)
            {
                if (registers[i] == register)
                {
                    if ((info.GetUsage(i) & RegisterFlags.Source) == RegisterFlags.Source)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is the given register used as destination in the given instruction?
        /// </summary>
        public static bool IsDestinationIn(this Register register, Instruction ins)
        {
            var registers = ins.Registers;
            var count = registers.Count;
            if (count == 0)
                return false;
            var info = OpCodeInfo.Get(ins.OpCode);
            for (var i = 0; i < count; i++)
            {
                if (registers[i] == register)
                {
                    if ((info.GetUsage(i) & RegisterFlags.Destination) == RegisterFlags.Destination)
                        return true;
                }
            }
            return false;
        }
    }
}
