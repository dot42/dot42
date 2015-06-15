using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Share constants values in registers as much as possible.
    /// </summary>
    internal class ShareConstTransformation : IRLTransformation
    {
        private const int MaxRegisters = 64;
        private const int LargeBlockSize = 64;

        /// <summary>
        /// Transform the given body.
        /// </summary>
        public bool Transform(Dex target, MethodBody body)
        {
            var basicBlocks = BasicBlock.Find(body);
            var registerCount = body.Registers.Count();
            if (registerCount > MaxRegisters)
                return false;

            Dictionary<Instruction, ConstantKey> allConstInstructions;
            CollectReadOnlyConstInstructions(body, out allConstInstructions);
            RegisterUsageMap registerUsageMap = null;

            foreach (var block in basicBlocks)
            {
                // Select all const instructions that have a next instruction in the block.
                var list = block.Instructions.ToList();
                var isLargeBlock = list.Count > LargeBlockSize;
                if (isLargeBlock)
                    continue;
                var constInstructionKeys = list.Where(x => allConstInstructions.ContainsKey(x))
                                                                               .Select(x => allConstInstructions[x])
                                                                               .ToList();

                // Select all const instructions where the next instruction is a move.
                while (constInstructionKeys.Count > 0)
                {
                    // Take the first instruction
                    var insKey = constInstructionKeys[0];
                    constInstructionKeys.RemoveAt(0);

                    // Get the register
                    var reg = insKey.Instruction.Registers[0];

                    // Are there other const instructions of the same type and the same operand?
                    var sameConstInstructions = constInstructionKeys.Where(x => x.Equals(insKey)).ToList();
                    if (sameConstInstructions.Count == 0)
                        continue;
                    if (sameConstInstructions.Count < 4)
                        continue;

                    // It is now save to use the register for all const operations
                    foreach (var other in sameConstInstructions)
                    {
                        // Replace all register uses
                        var otherReg = other.Instruction.Registers[0];

                        // Find usages
                        registerUsageMap = registerUsageMap ?? new RegisterUsageMap(body.Instructions);
                        registerUsageMap.ReplaceRegisterWith(otherReg, reg, body);

                        // Remove const
                        constInstructionKeys.Remove(other);
                        allConstInstructions.Remove(other.Instruction);

                        // Change const to nop which will be removed later
                        other.Instruction.ConvertToNop();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find all instructions that  are "const" and have a destination register that is never assigned to.
        /// </summary>
        private static void CollectReadOnlyConstInstructions(MethodBody body, out Dictionary<Instruction, ConstantKey> constInstructions)
        {
            // Find all const instructions
            constInstructions = body.Instructions
                                    .Where(ins => ins.Code.IsConst() && !ins.Registers[0].PreventOptimization)
                                    .ToDictionary(ins => ins, ins => new ConstantKey(ins));
            if (constInstructions.Count == 0)
                return;

            // Select all registers used by the const instructions
            var constRegs = new Dictionary<Register, Instruction>();
            var toRemove = new HashSet<Instruction>();
            foreach (var ins in constInstructions.Keys)
            {
                var reg = ins.Registers[0];
                if (constRegs.ContainsKey(reg))
                {
                    // Oops, duplicate register, remove this one
                    toRemove.Add(ins);
                    toRemove.Add(constRegs[reg]);
                }
                else
                {
                    // Add it
                    constRegs[reg] = ins;
                }
            }
            foreach (var ins in toRemove)
            {
                constInstructions.Remove(ins);
                constRegs.Remove(ins.Registers[0]);
            }
            foreach (var ins in body.Instructions)
            {
                if (constInstructions.ContainsKey(ins))
                    continue;

                var insRegs = ins.Registers;
                for (var k = 0; k < insRegs.Count; k++)
                {
                    var reg = insRegs[k];
                    Instruction constInst;
                    if (constRegs.TryGetValue(reg, out constInst))
                    {
                        if (ins.IsDestinationRegister(k))
                        {
                            // Register is a destination in another instruction, the constInst is not readonly.
                            constInstructions.Remove(constInst);
                            constRegs.Remove(reg);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Is the given instruction a const instruction which register is never modified.
        /// </summary>
        private static bool IsReadOnlyConst(Instruction ins, IInstructionRange range)
        {
            if (!ins.Code.IsConst())
                return false;
            var reg = ins.Registers[0];
            if (range.Instructions.Any(x => (x != ins) && reg.IsDestinationIn(x)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Are both instructions the same const operation (wrt code and value)?
        /// </summary>
        private static bool IsSameConst(Instruction a, Instruction b)
        {
            // Compare code
            if (a.Code != b.Code) return false;

            // Compare register type
            if (a.Registers[0].Type != b.Registers[0].Type)
                return false;

            // Compare const value
            var opA = a.Operand;
            var opB = b.Operand;
            if (!Equals(opA, opB) || (opA.GetType() != opB.GetType()))
                return false;
            
            return true;
        }

        /// <summary>
        /// Fast key for const instruction
        /// </summary>
        private sealed class ConstantKey
        {
            private readonly Instruction ins;
            private readonly int hashCode;

            public ConstantKey(Instruction ins)
            {
                this.ins = ins;
                hashCode = ((int) ins.Code << 24) ^ ((int) ins.Registers[0].Type << 22) ^ ins.Operand.GetHashCode();
            }

            public Instruction Instruction { get { return ins; } }

            public override int GetHashCode()
            {
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                var other = obj as ConstantKey;
                if ((other == null) || (other.hashCode != hashCode))
                    return false;

                // Compare code
                if (ins.Code != other.ins.Code) return false;

                // Compare register type
                if (ins.Registers[0].Type != other.ins.Registers[0].Type)
                    return false;

                // Compare const value
                var opA = ins.Operand;
                var opB = other.ins.Operand;
                if (!Equals(opA, opB) || (opA.GetType() != opB.GetType()))
                    return false;

                return true;
            }
        }
    }
}
