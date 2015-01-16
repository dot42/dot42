using System.Collections.Generic;
using System.Diagnostics;
using Dot42.CompilerLib.RL.Extensions;

namespace Dot42.CompilerLib.RL.Transformations
{
    [DebuggerDisplay("{Register} {Range}")]
    internal class RegisterRange
    {
        public Register Register { get; private set; }
        public IInstructionRange Range;
        public int InstructionCount; // Number of instructions using the register
        private HashSet<BasicBlock> blocks;

        public RegisterRange(Register register, IInstructionRange range)
        {
            Register = register;
            Range = range;
        }

        /// <summary>
        /// Add a block.
        /// </summary>
        public void AddBlock(BasicBlock block)
        {
            blocks = blocks ?? new HashSet<BasicBlock>();
            blocks.Add(block);
        }

        /// <summary>
        /// Does this range contain exactly one block (the given one).
        /// </summary>
        public bool ContainsExactlyOne(BasicBlock block)
        {
            if ((blocks == null) || (blocks.Count != 1))
                return false;
            return blocks.Contains(block);
        }

        /// <summary>
        /// Is the register of this range assigned in the first instruction of the range.
        /// </summary>
        public bool IsRegisterAssignedInFirstInstruction
        {
            get { return Register.IsDestinationIn(Range.First); }
        }

        /// <summary>
        /// Change all references to my register with the given register.
        /// </summary>
        public void ReplaceRegisterWith(Register oldRegister, Register newRegister, MethodBody body)
        {
            var replacement = (Register == oldRegister) ? newRegister : Register;
            Range.Instructions.ReplaceRegisterWith(oldRegister, newRegister, body);
            Register = replacement;
        }
    }
}