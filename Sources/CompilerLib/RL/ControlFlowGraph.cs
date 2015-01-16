using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;

namespace Dot42.CompilerLib.RL
{
    public class ControlFlowGraph : IEnumerable<BasicBlock>
    {
        private readonly List<BasicBlock> basicBlocks;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ControlFlowGraph(MethodBody body)
        {
            // Find all basic blocks
            basicBlocks = BasicBlock.Find(body);

            // Connect entry/exit blocks
            BasicBlock previousBlock = null;
            foreach (var block in basicBlocks)
            {
                var operand = block.Exit.Operand;
                Instruction ins;
                Instruction[] insArray;
                Tuple<int, Instruction>[] tupleArray;
                if ((ins = operand as Instruction) != null)
                {
                    // Find block for this instruction
                    var target = basicBlocks.First(x => x.Contains(ins));
                    AddExitTarget(block, target);
                }
                else if ((insArray = operand as Instruction[]) != null)
                {
                    // Find block for each instruction
                    foreach (var xins in insArray)
                    {
                        var target = basicBlocks.First(x => x.Contains(xins));
                        AddExitTarget(block, target);
                    }
                }
                else if ((tupleArray = operand as Tuple<int, Instruction>[]) != null)
                {
                    // Find block for each instruction
                    foreach (var tuple in tupleArray)
                    {
                        var target = basicBlocks.First(x => x.Contains(tuple.Item2));
                        AddExitTarget(block, target);
                    }
                }

                // Attach previous block in case of conditional branch
                if (previousBlock != null)
                {
                    var exitCode = previousBlock.Exit.Code;
                    if (!(exitCode.IsUnconditionalBranch() || exitCode.IsReturn()))
                    {
                        AddExitTarget(previousBlock, block);
                    }
                }

                // Remember
                previousBlock = block;
            }
        }

        private static void AddExitTarget(BasicBlock current, BasicBlock target)
        {
            current.AddExitBlock(target);
            target.AddEntryBlock(current);
        }

        public int Count { get { return basicBlocks.Count; } }

        public BasicBlock this[int index]
        {
            get { return basicBlocks[index]; }
        }

        public IEnumerator<BasicBlock> GetEnumerator()
        {
            return basicBlocks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
