using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.RL2DexCompiler.Extensions;
using Dot42.DexLib.Instructions;
using Instruction = Dot42.DexLib.Instructions.Instruction;
using MethodBody = Dot42.DexLib.Instructions.MethodBody;
using Register = Dot42.DexLib.Instructions.Register;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    internal static class RegisterSpilling
    {
        /// <summary>
        /// Add instructions that spill registers (when needed).
        /// </summary>
        internal static void AddSpillingCode(MethodBody body, RegisterMapper mapper)
        {
            // Calculate the basic blocks
            var basicBlocks = BasicBlock.Find(body);
            var spillRegisters = mapper.SpillRegisters.ToList();
            var invokeFrame = mapper.InvocationFrameRegisters.ToList();
            var allRegisters = mapper.All.ToList();

            foreach (var block in basicBlocks)
            {
                var generator = new BlockSpillCodeGenerator(block, mapper, spillRegisters, invokeFrame, allRegisters);
                generator.Generate(body);
                generator.FinalizeBlock(body);
            }
        }

        /// <summary>
        /// Are the given registers a valid range for use in invoke_x_range?
        /// </summary>
        internal static bool IsValidInvokeRange(IEnumerable<Register> registers)
        {
            var first = true;
            var nextIndex = 0;

            foreach (var reg in registers)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (reg.Index != nextIndex)
                        return false;
                }
                nextIndex = reg.Index + 1;
            }
            return true;
        }

        /// <summary>
        /// Helper used to add spilling code to a basic block.
        /// </summary>
        private class BlockSpillCodeGenerator
        {
            private readonly BasicBlock block;
            private readonly RegisterMapper mapper;
            private readonly LowRegisterState[] lowRegisters;
            private readonly List<Register> invokeFrame;
            private int nextSpillIndex;
            private readonly List<Register> allRegisters;

            /// <summary>
            /// Default ctor.
            /// </summary>
            internal BlockSpillCodeGenerator(BasicBlock block, RegisterMapper mapper, List<Register> lowRegisters, List<Register> invokeFrame, List<Register> allRegisters)
            {
                this.block = block;
                this.mapper = mapper;
                this.lowRegisters = new LowRegisterState[lowRegisters.Count];
                LowRegisterState next = null;
                for (var i = lowRegisters.Count - 1; i >= 0; i--)
                {
                    this.lowRegisters[i] = new LowRegisterState(lowRegisters[i], next);
                    next = this.lowRegisters[i];
                }
                this.invokeFrame = invokeFrame;
                this.allRegisters = allRegisters;
            }

            /// <summary>
            /// Add instructions that spill registers (when needed).
            /// </summary>
            internal void Generate(MethodBody body)
            {
                // Go over each instruction in the block.
                var instructions = block.Instructions.ToList();
                foreach (var ins in instructions)
                {
                    var registers = ins.Registers;
                    var count = registers.Count;
                    if (count == 0)
                    {
                        // No registers, so no spilling
                        continue;
                    }

                    var isInvoke = ins.OpCode.IsInvoke();
                    if (isInvoke)
                    {
                        if (ins.RequiresInvokeRange())
                        {
                            ConvertInvoke(ins);
                        }
                    }
                    else
                    {
                        var info = OpCodeInfo.Get(ins.OpCode);
                        for (var i = 0; i < count; i++)
                        {
                            Register r;
                            LowRegisterState state;
                            var spill = TryGetLowRegister(registers[i], out r, out state);
                            if (!spill)
                            {
                                var size = info.GetUsage(i) & RegisterFlags.SizeMask;
                                switch (size)
                                {
                                    case RegisterFlags.Bits4:
                                        spill = !registers[i].IsBits4;
                                        break;
                                    case RegisterFlags.Bits8:
                                        spill = !registers[i].IsBits8;
                                        break;
                                }
                            }
                            if (spill)
                            {
                                // Insert spilling code
                                AddCode(ins, registers[i], body);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Add code to save low register to their original registers if needed.
            /// </summary>
            public void FinalizeBlock(MethodBody body)
            {
                Instruction endingNop = null;
                foreach (var lowRegState in lowRegisters.Where(x => x.SaveToOriginalNeeded))
                {
                    if (endingNop == null)
                    {
                        endingNop = new Instruction(OpCodes.Nop);
                        // Block exit can never be a branch target, so no need to reroute.
                        if (block.Exit.OpCode.IsBranch())
                            block.InsertBefore(block.Exit, endingNop);
                        else
                            block.InsertAfter(block.Exit, endingNop);
                    }
                    // Add move instruction to save low register to original
                    lowRegState.SaveToOriginalAndClear(endingNop, block, body, mapper.RegisterSpillingMap);
                }
            }

            /// <summary>
            /// Try to get the low register that the given original register is mapped onto.
            /// </summary>
            private bool TryGetLowRegister(Register originalRegister, out Register lowRegister, out LowRegisterState lowRegisterState)
            {
                lowRegisterState = lowRegisters.FirstOrDefault(x => x.OriginalRegister == originalRegister);
                lowRegister = (lowRegisterState != null) ? lowRegisterState.LowRegister : null;
                return (lowRegister != null);
            }

            /// <summary>
            /// Gets the type stored in the given register.
            /// </summary>
            private RType GetType(Register register)
            {
                var lowState = lowRegisters.FirstOrDefault(x => x.LowRegister == register);
                return mapper.GetType(lowState != null ? lowState.OriginalRegister : register);                
            }

            /// <summary>
            /// Convert a normal invoke_x to invoke_x_range.
            /// </summary>
            private void ConvertInvoke(Instruction ins)
            {
                var registers = ins.Registers;
                var count = registers.Count;
                if (count == 0)
                    return;

                // Replace all registers with their "spill" variant (if any)
                for (var i = 0; i < count; i++)
                {
                    Register sReg;
                    LowRegisterState state;
                    if (TryGetLowRegister(registers[i], out sReg, out state))
                    {
                        ReplaceRegister(ins, registers[i], sReg);
                    }
                }

                // By replacing the registers, it it possible to avoid the invoke_x_range opcodes?
                if (!ins.RequiresInvokeRange())
                    return;

                if (!IsValidInvokeRange(registers))
                {
                    // Use invoke frame
                    for (var i = 0; i < count; i++)
                    {
                        var type = GetType(registers[i]);
                        block.InsertBefore(ins, CreateMove(registers[i], invokeFrame[i], type));
                        registers[i] = invokeFrame[i];
                    }
                }
                // Change opcode
                ins.OpCode = ins.OpCode.InvokeToRange();
            }

            /// <summary>
            /// Add instructions that spill the given register.
            /// </summary>
            private void AddCode(Instruction ins, Register register, MethodBody body)
            {
                // Try to find an earlier map for the register
                {
                    Register sReg;
                    LowRegisterState state;
                    if (TryGetLowRegister(register, out sReg, out state))
                    {
                        // Already mapped, just replace.
                        ReplaceRegister(ins, register, sReg);

                        // Should we save the low register now?
                        if ((!state.SaveToOriginalNeeded) && (sReg.IsDestinationIn(ins)))
                        {
                            state.SaveToOriginalNeeded = true;
                        }
                        return;
                    }
                }

                // Allocate low register
                var sRegState = AllocateLowRegister(ins, register, body);

                // We can now use sReg as replacement for register.
                // Add move (only if the register is a source of the instruction)
                if (register.IsSourceIn(ins))
                {
                    block.InsertBefore(ins, CreateMove(register, sRegState.LowRegister, GetType(register)));
                }
                // See if we need to save the low register afterwards
                if ((!sRegState.SaveToOriginalNeeded) && (register.IsDestinationIn(ins)))
                {
                    sRegState.SaveToOriginalNeeded = true;
                }
                // Replace the register
                ReplaceRegister(ins, register, sRegState.LowRegister);

                // Record start point in mapping
                sRegState.Mapping.FirstInstruction = ins;
            }

            /// <summary>
            /// Allocate a low register and map it to the given original register.
            /// </summary>
            private LowRegisterState AllocateLowRegister(Instruction ins, Register register, MethodBody body)
            {
                // Prepare
                var type = GetType(register);
                var isWide = (type == RType.Wide);
                var regsNeeded = isWide ? 2 : 1;

                LowRegisterState regState = null;
                HashSet<Register> allRegistersUsedInIns = null;
                while (true)
                {
                    // Try to allocate free spilling register
                    regState = lowRegisters.FirstOrDefault(x => x.IsFreeFor(type));
                    if (regState != null)
                        break; // Found a free low register

                    // No spilling register is available.
                    // Free another register first
                    // If wide, free 2 registers
                    if (nextSpillIndex + regsNeeded > lowRegisters.Length) nextSpillIndex = 0;
                    regState = lowRegisters[nextSpillIndex];
                    var regStateNext = isWide ? lowRegisters[nextSpillIndex + 1] : null;

                    // Update index so we take another the next time
                    nextSpillIndex = (nextSpillIndex + regsNeeded) % (lowRegisters.Length - 1);

                    // Do not allocate registers already used in the current instruction
                    allRegistersUsedInIns = allRegistersUsedInIns ?? GetAllUsedRegisters(ins);
                    if (allRegistersUsedInIns.Contains(regState.LowRegister))
                        continue;
                    if ((regStateNext != null) && allRegistersUsedInIns.Contains(regStateNext.LowRegister))
                        continue;
             
                    // Save sReg back to original register
                    var map = mapper.RegisterSpillingMap;
                    regState.SaveToOriginalAndClear(ins, block, body, map);
                    if (regStateNext != null) regStateNext.SaveToOriginalAndClear(ins, block, body, map);
                }
                // Add mapping
                regState.SetInUseBy(register, type);
                return regState;
            }

            /// <summary>
            /// Gets all registers used in the given instruction, include "next" registers in case of wide operands.
            /// </summary>
            private HashSet<Register> GetAllUsedRegisters(Instruction ins)
            {
                var set = new HashSet<Register>();
                foreach (var r in ins.Registers)
                {
                    set.Add(r);
                    if (GetType(r) == RType.Wide)
                    {
                        // Include next register
                        var next = allRegisters.First(x => x.Index == r.Index + 1);
                        set.Add(next);
                    }
                }
                return set;
            }

            /// <summary>
            /// Create a move instruction.
            /// </summary>
            private static Instruction CreateMove(Register from, Register to, RType type)
            {
                if (from == null)
                    throw new ArgumentNullException("from");
                if (to == null)
                    throw new ArgumentNullException("to");
                OpCodes opcode;
                switch (type)
                {
                    case RType.Value:
                        opcode = OpCodes.Move;
                        break;
                    case RType.Wide:
                        opcode = OpCodes.Move_wide;
                        break;
                    case RType.Wide2:
                        return new Instruction(OpCodes.Nop);
                    case RType.Object:
                        opcode = OpCodes.Move_object;
                        break;
                    default:
                        throw new ArgumentException("Unknown type: " + (int)type);
                }
                return new Instruction(opcode, to, @from);
            }

            /// <summary>
            /// Replace all occurrences of from to to in the register list of the given instruction.
            /// </summary>
            private static void ReplaceRegister(Instruction ins, Register from, Register to)
            {
                if (from == null)
                    throw new ArgumentNullException("from");
                if (to == null)
                    throw new ArgumentNullException("to");
                var registers = ins.Registers;
                var count = registers.Count;
                for (var i = 0; i < count; i++)
                {
                    if (registers[i] == from)
                    {
                        registers[i] = to;
                    }
                }
            }

            /// <summary>
            /// State of a low register at a specific moment in time.
            /// </summary>
            [DebuggerDisplay("{OriginalRegister}->{LowRegister} {SaveToOriginalNeeded}")]
            private sealed class LowRegisterState
            {
                /// <summary>
                /// The low register this state is about
                /// </summary>
                public readonly Register LowRegister;

                /// <summary>
                /// Link to the next low register state (is null for the last)
                /// </summary>
                private readonly LowRegisterState next;

                /// <summary>
                /// The "high" register that is currently mapped to this low register (can be null).
                /// </summary>
                public Register OriginalRegister { get; private set; }

                /// <summary>
                /// The current mapping for this entry
                /// </summary>
                public RegisterSpillingMapping Mapping { get; private set; }

                /// <summary>
                /// If true, the value of the low register must be copied to the original register.
                /// </summary>
                public bool SaveToOriginalNeeded;

                /// <summary>
                /// Is this low register being used as the first register of a wide value?
                /// </summary>
                private bool isWide;

                /// <summary>
                /// Type currently in this register
                /// </summary>
                private RType type;

                /// <summary>
                /// If set, the register state holds the start of the wide value that this register state holds the second part of.
                /// </summary>
                private LowRegisterState wideStart;

                /// <summary>
                /// Default ctor
                /// </summary>
                public LowRegisterState(Register lowRegister, LowRegisterState next)
                {
                    LowRegister = lowRegister;
                    this.next = next;
                }

                /// <summary>
                /// Is this low register currently available for the given type of register?
                /// </summary>
                public bool IsFreeFor(RType type)
                {
                    if (OriginalRegister != null) return false;
                    if (type == RType.Wide) return (next != null) && (next.OriginalRegister == null);
                    return true;
                } 

                /// <summary>
                /// Mark this low register state as no longer in use.
                /// </summary>
                private void Clear()
                {
                    if (isWide) next.Clear();
                    OriginalRegister = null;
                    SaveToOriginalNeeded = false;
                    isWide = false;
                    type = RType.Value;
                    wideStart = null;
                    Mapping = null;
                }

                /// <summary>
                /// Mark this low register as in use by the given original register.
                /// </summary>
                public void SetInUseBy(Register originalRegister, RType type)
                {
                    if (OriginalRegister != null)
                        throw new InvalidOperationException("Register state is still in use");
                    if (type == RType.Wide)
                    {
                        if (next == null)
                            throw new InvalidOperationException("Register state cannot hold a wide");
                        if (next.OriginalRegister != null)
                            throw new InvalidOperationException("Register state.next is still in use");
                    }
                    OriginalRegister = originalRegister;
                    isWide = (type == RType.Wide);
                    this.type = type;
                    if (isWide)
                    {
                        next.isWide = false;
                        next.OriginalRegister = originalRegister;
                        next.type = RType.Wide2;
                        next.wideStart = this;
                    }
                    Mapping = new RegisterSpillingMapping(OriginalRegister, LowRegister);
                }

                /// <summary>
                /// Save the state in the low register back to the original register when a save is needed.
                /// The save code is inserted directly before the given target instruction.
                /// </summary>
                public void SaveToOriginalAndClear(Instruction target, BasicBlock block, MethodBody body, RegisterSpillingMap map)
                {
                    if (wideStart != null)
                    {
                        wideStart.SaveToOriginalAndClear(target, block, body, map);
                    }
                    else
                    {
                        if (SaveToOriginalNeeded)
                        {
                            var original = OriginalRegister;
                            // Fix target to insert before
                            if (target.OpCode.IsMoveResult())
                            {
                                do
                                {
                                    target = target.GetPrevious(body.Instructions);
                                } while (target.OpCode == OpCodes.Nop);
                            }
                            // Insert save code
                            var move = CreateMove(LowRegister, original, type);
                            block.InsertBefore(target, move);
                            // Save end in mapping
                            Mapping.LastInstruction = move;
                            // Record mapping
                            map.Add(Mapping);
                        }
                        Clear();
                    }
                }
            }
        }
    }
}