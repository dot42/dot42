using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL.Transformations
{
    /// <summary>
    /// Make sure all registers are initialized.
    /// </summary>
    internal class InitializeRegistersTransformation : IRLTransformation
    {
        /// <summary>
        /// Transform the given body.
        /// </summary>
        public bool Transform(Dex target, MethodBody body)
        {
            // Build the control flow graph
            var cfg = new ControlFlowGraph(body);

            // Go over each block to find registers that are initialized and may need initialization
            foreach (var iterator in cfg)
            {
                var block = iterator;
                var data = new BasicBlockData(block);
                block.Tag = data;

                // Go over all instructions in the block, finding source/destination registers
                foreach (var ins in block.Instructions)
                {
                    var info = OpCodeInfo.Get(ins.Code.ToDex());
                    var count = ins.Registers.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var reg = ins.Registers[i];
                        if (reg.Category != RCategory.Argument)
                        {
                            var flags = info.GetUsage(i);
                            if (flags.HasFlag(RegisterFlags.Source))
                            {
                                // Must be initialize
                                if (!data.Initialized.Contains(reg))
                                {
                                    data.MayNeedInitialization.Add(reg);
                                }
                            }
                            if (flags.HasFlag(RegisterFlags.Destination))
                            {
                                // Destination
                                data.Initialized.Add(reg);
                            }
                        }
                    }
                }

            }

            // Go over all blocks to collect the register that really need initialization
            var needInitialization = new HashSet<Register>();
            var firstBlockData = (BasicBlockData)cfg[0].Tag;

            foreach (var iterator in cfg)
            {
                var block = iterator;
                var data = (BasicBlockData)block.Tag;

                foreach (var regIterator in data.MayNeedInitialization)
                {
                    // Short cut
                    var reg = regIterator;
                    if (needInitialization.Contains(reg))
                        continue;

                    bool isInitializedInFirstBlock = block != cfg[0] && firstBlockData.IsInitialized(reg);

                    // If the register is initialized in all entry blocks, we do not need to initialize it
                    // Note that this code is not complete: if a register is initialize in all entry blocks
                    // of the checked entry block, it should be treated as initialized as well.
                    // For simplification, we just check initialization in the very first block though.
                    if (!isInitializedInFirstBlock && block.EntryBlocks.Select(x => (BasicBlockData)x.Tag).Any(x => !x.IsInitialized(reg)))
                    {
                        // There is an entry block that does not initialize the register, so we have to initialize it
                        needInitialization.Add(reg);
                    }
                }
            }

            var index = 0;
            Register valueReg = null;
            Register objectReg = null;
            Register wideReg = null;
            var firstSourceLocation = body.Instructions[0].SequencePoint;
            foreach (var reg in needInitialization.OrderBy(x => x.Index))
            {
                switch (reg.Type)
                {
                    case RType.Value:
                        if (valueReg == null)
                        {
                            body.Instructions.Insert(index++, new Instruction(RCode.Const, 0, new[] {reg}) { SequencePoint = firstSourceLocation });
                            valueReg = reg;
                        }
                        else if (valueReg != reg)
                        {
                            body.Instructions.Insert(index++, new Instruction(RCode.Move, reg, valueReg) { SequencePoint = firstSourceLocation });                            
                        }
                        break;
                    case RType.Object:
                        if (objectReg == null)
                        {
                            body.Instructions.Insert(index++, new Instruction(RCode.Const, 0, new[] { reg }) { SequencePoint = firstSourceLocation });
                            objectReg = reg;
                        }
                        else if (objectReg != reg)
                        {
                            body.Instructions.Insert(index++, new Instruction(RCode.Move_object, reg, objectReg) { SequencePoint = firstSourceLocation });
                        }
                        break;
                    case RType.Wide:
                        if (wideReg == null)
                        {
                            body.Instructions.Insert(index++, new Instruction(RCode.Const_wide, 0, new[] { reg }) { SequencePoint = firstSourceLocation });
                            wideReg = reg;
                        }
                        else if (wideReg != reg)
                        {
                            body.Instructions.Insert(index++, new Instruction(RCode.Move_wide, reg, wideReg) { SequencePoint = firstSourceLocation });
                        }
                        break;
                }
            }

            return false;
        }

        private class BasicBlockData
        {
            public readonly BasicBlock Block;
            public readonly HashSet<Register> Initialized = new HashSet<Register>();
            public readonly HashSet<Register> MayNeedInitialization = new HashSet<Register>();
            private HashSet<BasicBlockData> allEntryBlocks;

            /// <summary>
            /// Default ctor
            /// </summary>
            public BasicBlockData(BasicBlock block)
            {
                Block = block;
            }

            /// <summary>
            /// Is the given register initialized in this block or all entry blocks.
            /// </summary>
            public bool IsInitialized(Register reg)
            {
                if (Initialized.Contains(reg))
                    return true;
                var entryBlocks = GetAllEntryBlocks();
                if (entryBlocks.Contains(this))
                    return false; // Possible recursion and the register is not initialized here
                return entryBlocks.All(x => x.IsInitialized(reg));
            }

            private HashSet<BasicBlockData> GetAllEntryBlocks()
            {
                if (allEntryBlocks == null)
                {
                    var set = new HashSet<BasicBlockData>();
                    CollectEntryBlocks(Block, set);
                    allEntryBlocks = set;
                }
                return allEntryBlocks;
            }

            private static void CollectEntryBlocks(BasicBlock current, HashSet<BasicBlockData> set)
            {
                var data = (BasicBlockData) current.Tag;
                if (!set.Add(data))
                    return; // Already added
                foreach (var entryBlock in current.EntryBlocks)
                {
                    CollectEntryBlocks(entryBlock, set);
                }
            }
        }
    }
}
