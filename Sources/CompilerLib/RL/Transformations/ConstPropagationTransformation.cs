using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Resolve const-move as must as possible.
    /// </summary>
    internal class ConstPropagationTransformation : IRLTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        public bool Transform(Dex target, MethodBody body)
        {
            bool hasChanges = false;

            // Find all "const" instructions and record register usage
            var allConstInstructions = new List<Instruction>();
            var registerUsage = new Dictionary<Register, List<Instruction>>();
            CollectInstructionInformation(body, allConstInstructions, registerUsage);

            // Find all basic blocks
            var basicBlocks = BasicBlock.Find(body);

            // Go over each block
            foreach (var iterator in basicBlocks)
            {
                var block = iterator;
                // Select all const instructions where the next instruction is a move.
                foreach (var ins in allConstInstructions)
                {
                    var r = ins.Registers[0];
                    // Only optimize const instructions to temp registers
                    if (r.Category != RCategory.Temp)
                        continue;

                    // Get all instructions using this register
                    var all = registerUsage[r];
                    if ((all.Count != 2) || (all[0] != ins))
                        continue;
                    var next = all[1];
                    // Opcode must match
                    if (next.Code != ConstToMove(ins.Code))
                        continue;
                    // Register must match
                    if (next.Registers[1] != r)
                        continue;
                    
                    // The following are the most expensive, so only test them if we have to.
                    // Only optimize is instruction is in current block
                    if (!block.Contains(ins))
                        continue;

                    // Next must be in same basic block
                    if (!block.Contains(next))
                        continue;

                    // We found a replacement
                    r = next.Registers[0];
                    ins.Registers[0] = r;
                    next.ConvertToNop();

                    hasChanges = true;
                }
            }
            return hasChanges;
        }

        /// <summary>
        /// Go over all instructions in the given body and collect all const-instructions and record instructions that use a specific register.
        /// </summary>
        private static void CollectInstructionInformation(MethodBody body, List<Instruction> constInstructions, Dictionary<Register, List<Instruction>> registerUsage)
        {
            foreach (var instr in body.Instructions)
            {
                if (instr.Code.IsConst())
                {
                    constInstructions.Add(instr);
                }
                foreach (var reg in instr.Registers)
                {
                    List<Instruction> usages;
                    if (!registerUsage.TryGetValue(reg, out usages))
                    {
                        usages = new List<Instruction>();
                        registerUsage.Add(reg, usages);
                    }
                    usages.Add(instr);
                }
            }
        }

        /// <summary>
        /// Gets the move code that matches the given const predecessor code.
        /// </summary>
        private static RCode ConstToMove(RCode code)
        {
            switch (code)
            {
                case RCode.Const:
                    return RCode.Move;
                case RCode.Const_class:
                    return RCode.Move_object;
                case RCode.Const_wide:
                    return RCode.Move_wide;
                case RCode.Const_string:
                    return RCode.Move_object;
                default:
                    throw new ArgumentException("Invalid code");
            }
        }
    }
}
