using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.DexLib.Instructions;
using Dot42.Utility;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    /// <summary>
    /// A basic block has no entries other than the first instruction and
    /// no other exits than the last instruction.
    /// </summary>
    [DebuggerDisplay("{EntryIndex} - {ExitIndex}")]
    public sealed class BasicBlock  : IInstructionRange
    {
        private readonly IList<Instruction> instructions;
        private readonly Instruction entry;
        private readonly Instruction exit;
        private HashSet<BasicBlock> entryBlocks;
        private HashSet<BasicBlock> exitBlocks;
        private HashSet<Register> liveRegistersAtExit;

        /// <summary>
        /// Default ctor
        /// </summary>
        private BasicBlock(IList<Instruction> instructions, Instruction entry, Instruction exit)
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
        /// Index in instruction list of entry.
        /// </summary>
        public int EntryIndex { get { return instructions.IndexOf(entry); } }

        /// <summary>
        /// Gets the last instruction of the block.
        /// </summary>
        public Instruction Exit { get { return exit; } }

        /// <summary>
        /// Index in instruction list of exit.
        /// </summary>
        public int ExitIndex { get { return instructions.IndexOf(exit); } }

        /// <summary>
        /// Gets/sets the blocks that jump into this block.
        /// </summary>
        public IEnumerable<BasicBlock> EntryBlocks { get { return entryBlocks ?? Enumerable.Empty<BasicBlock>(); } }

        /// <summary>
        /// Gets/sets the blocks this block can jump to.
        /// </summary>
        public IEnumerable<BasicBlock> ExitBlocks { get { return exitBlocks ?? Enumerable.Empty<BasicBlock>(); } }

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
        /// Gets all instructions in this block.
        /// </summary>
        public IEnumerable<Instruction> Instructions
        {
            get
            {
                var entryIndex = instructions.IndexOf(entry);
                var exitIndex = instructions.IndexOf(exit);
                for (var i = entryIndex; i <= exitIndex; i++)
                {
                    yield return instructions[i];
                }
            }
        }

        /// <summary>
        /// Insert the given instruction just after the given target instruction.
        /// </summary>
        public void InsertAfter(Instruction target, Instruction ins)
        {
            var index = instructions.IndexOf(target) + 1;
            instructions.Insert(index, ins);
        }

        /// <summary>
        /// Insert the given instruction just before the given target instruction.
        /// </summary>
        public void InsertBefore(Instruction target, Instruction ins)
        {
            var index = instructions.IndexOf(target);
            instructions.Insert(index, ins);
        }

        /// <summary>
        /// Does this block contain the given instruction?
        /// </summary>
        public bool Contains(Instruction i)
        {
            var iIndex = instructions.IndexOf(i);
            if (iIndex < EntryIndex)
                return false;
            if (iIndex > ExitIndex)
                return false;
            return true;
        }

        /// <summary>
        /// Is the given register live at the start of this block?
        /// </summary>
        public bool IsLiveAtEntry(Register r)
        {
            return EntryBlocks.Any() && EntryBlocks.All(x => x.IsLiveAtExit(r));
        }

        /// <summary>
        /// Is the given register live at the end of this block?
        /// </summary>
        public bool IsLiveAtExit(Register r)
        {
            return (liveRegistersAtExit != null) && liveRegistersAtExit.Contains(r);
        }

        /// <summary>
        /// Add the given register to the set of registers that is live at the end of this block.
        /// </summary>
        public void AddLiveRegisterAtExit(Register r)
        {
            liveRegistersAtExit = liveRegistersAtExit ?? new HashSet<Register>();
            liveRegistersAtExit.Add(r);

            // Update exit blocks
            foreach (var exitBlock in ExitBlocks)
            {
                if ((exitBlock == this) || exitBlock.IsLiveAtExit(r))
                    continue;
                if (exitBlock.IsLiveAtEntry(r))
                {
                    exitBlock.AddLiveRegisterAtExit(r);
                }
            }
        }

        /// <summary>
        /// Find all basic blocks in the given body.
        /// </summary>
        public static List<BasicBlock> Find(MethodBody body)
        {
            var bodyInstructions = body.Instructions;
            if (bodyInstructions.Count == 0)
                return new List<BasicBlock>();
            var instructions = new IndexLookupList<Instruction>(bodyInstructions);

            var targetInstructions = bodyInstructions.Select(x => x.Operand).OfType<Instruction>().ToList();
            targetInstructions.AddRange(bodyInstructions.Select(x => x.Operand).OfType<ISwitchData>().SelectMany(x => x.GetTargets()));
            targetInstructions.AddRange(bodyInstructions.Where(x => x.OpCode.IsBranch()).Select(x => GetNextOrDefault(instructions, x)));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.TryStart));
            targetInstructions.AddRange(body.Exceptions.Select(x => GetNextOrDefault(instructions, x.TryEnd)));
            targetInstructions.AddRange(body.Exceptions.SelectMany(x => x.Catches, (h, y) => y.Instruction));
            targetInstructions.AddRange(body.Exceptions.Select(x => x.CatchAll));
            targetInstructions.Add(body.Instructions[0]);

            // Get sorted list with duplicates removed.
            var startInstructions = targetInstructions.Where(x => x != null).Distinct().OrderBy(instructions.IndexOf).ToList();
            var result = new List<BasicBlock>();
            for (var i = 0; i < startInstructions.Count; i++)
            {
                var entry = startInstructions[i];
                var exit = (i + 1 < startInstructions.Count)
                               ? GetPrevious(instructions, startInstructions[i + 1])
                               : body.Instructions[body.Instructions.Count - 1];
                result.Add(new BasicBlock(instructions, entry, exit));
            }
            return result;
        }

        /// <summary>
        /// Gets the instruction that directly follows the given instruction.
        /// Returns null if the given instruction is the last instruction.
        /// </summary>
        private static Instruction GetNextOrDefault(IList<Instruction> instructions, Instruction i)
        {
            var index = instructions.IndexOf(i) + 1;
            if (index < instructions.Count)
                return instructions[index];
            return null;
        }

        /// <summary>
        /// Gets the instruction that is directly in front of the given instruction.
        /// </summary>
        private static Instruction GetPrevious(IList<Instruction> instructions, Instruction i)
        {
            var index = instructions.IndexOf(i) - 1;
            return instructions[index];
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
