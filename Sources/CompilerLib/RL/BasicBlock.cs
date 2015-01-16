using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.RL.Extensions;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// A basic block has no entries other than the first instruction and
    /// no other exits than the last instruction.
    /// </summary>
    [DebuggerDisplay("{Entry} .. {Exit}")]
    public sealed class BasicBlock : IInstructionRange
    {
        private readonly InstructionList instructions;
        private readonly Instruction entry;
        private readonly Instruction exit;
        private HashSet<BasicBlock> entryBlocks;
        private HashSet<BasicBlock> exitBlocks;

        /// <summary>
        /// Default ctor
        /// </summary>
        private BasicBlock(InstructionList instructions, Instruction entry, Instruction exit)
        {
            this.instructions = instructions;
            this.entry = entry;
            this.exit = exit;
        }

        /// <summary>
        /// Gets the first instruction of the block.
        /// </summary>
        public Instruction Entry { get { return entry; } }

        /// <summary>
        /// Gets the last instruction of the block.
        /// </summary>
        public Instruction Exit { get { return exit; } }

        /// <summary>
        /// Gets all instructions in this block.
        /// </summary>
        public IEnumerable<Instruction> Instructions
        {
            get
            {
                var i = entry.Index;
                while (true)
                {
                    var current = instructions[i];
                    yield return current;
                    if (current == exit)
                        yield break;
                    i++;
                }
            }
        }

        /// <summary>
        /// Gets/sets the blocks that jump into this block.
        /// </summary>
        public IEnumerable<BasicBlock> EntryBlocks { get { return entryBlocks ?? Enumerable.Empty<BasicBlock>(); } }

        /// <summary>
        /// Gets/sets the blocks this block can jump to.
        /// </summary>
        public IEnumerable<BasicBlock> ExitBlocks { get { return exitBlocks ?? Enumerable.Empty<BasicBlock>(); } }

        /// <summary>
        /// Custom tag.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Does this block contain the given instruction?
        /// </summary>
        public bool Contains(Instruction i)
        {
            if (i.Parent != entry.Parent)
                return false;
            var iIndex = i.Index;
            if (iIndex < entry.Index)
                return false;
            if (iIndex > exit.Index)
                return false;
            return true;
        }

        /// <summary>
        /// Find all basic blocks in the given body.
        /// </summary>
        public static List<BasicBlock> Find(MethodBody body)
        {
            if (body.Instructions.Count == 0)
                return new List<BasicBlock>();

            // Tuple<int, Instruction>

            var targetInstructions = body.Instructions.Select(x => x.Operand).OfType<Instruction>().ToList();
            targetInstructions.AddRange(body.Instructions.Select(x => x.Operand).OfType<Instruction[]>().SelectMany(x => x));
            targetInstructions.AddRange(body.Instructions.Select(x => x.Operand).OfType<Tuple<int, Instruction>[]>().SelectMany(x => x).Select(x => x.Item2));
            targetInstructions.AddRange(body.Instructions.Where(x => x.Code.IsBranch() || x.Code.IsReturn()).Select(x => x.NextOrDefault));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.TryStart));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.TryEnd.Next));
            targetInstructions.AddRange(body.Exceptions.SelectMany(x => x.Catches, (h, y) => y.Instruction));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.CatchAll));
            targetInstructions.Add(body.Instructions[0]);

            // Get sorted list with duplicates removed.
            var startInstructions = targetInstructions.Where(x => x != null).Distinct().OrderBy(x => x.Index).ToList();
            var result = new List<BasicBlock>();
            for (var i = 0; i < startInstructions.Count; i++)
            {
                var entry = startInstructions[i];
                var exit = (i + 1 < startInstructions.Count)
                               ? startInstructions[i + 1].Previous
                               : body.Instructions[body.Instructions.Count - 1];
                result.Add(new BasicBlock(body.Instructions, entry, exit));
            }
            return result;
        }

        /// <summary>
        /// First instruction in the range.
        /// </summary>
        Instruction IInstructionRange.First
        {
            get { return entry; }
        }

        /// <summary>
        /// Last (inclusive) instruction in the range.
        /// </summary>
        Instruction IInstructionRange.Last
        {
            get { return exit; }
        }

        /// <summary>
        /// Does this block contain the entire range?
        /// </summary>
        /// <returns>False if any instruction of the given range is outside this block, true if all instructions of the given range are inside this block</returns>
        public bool ContainsEntireRange(IInstructionRange range)
        {
            var entryIndex = entry.Index;
            var exitIndex = exit.Index;
            foreach (var i in range.Instructions)
            {
                var index = i.Index;
                if ((index < entryIndex) || (index > exitIndex))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Add the given block to my list of entry blocks.
        /// </summary>
        internal void AddEntryBlock(BasicBlock entryBlock)
        {
            entryBlocks = entryBlocks ?? new HashSet<BasicBlock>();
            entryBlocks.Add(entryBlock);
        }

        /// <summary>
        /// Add the given block to my list of exit blocks.
        /// </summary>
        internal void AddExitBlock(BasicBlock exitBlock)
        {
            exitBlocks = exitBlocks ?? new HashSet<BasicBlock>();
            exitBlocks.Add(exitBlock);
        }
    }
}
