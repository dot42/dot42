using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// List of instructions.
    /// </summary>
    public sealed class InstructionList : IEnumerable<Instruction>, IInstructionRange, IRLBuilder
    {
        private readonly IndexLookupList<Instruction>  list = new IndexLookupList<Instruction>();
        internal int Modifications = 1;

        /// <summary>
        /// Gets the number of elements
        /// </summary>
        public int Count { get { return list.Count; } }

        /// <summary>
        /// Gets the instruction as the given index.
        /// </summary>
        public Instruction this[int index] { get { return list[index]; } }

        /// <summary>
        /// Add the given instruction to the end of this list.
        /// </summary>
        public void Add(Instruction instruction)
        {
            if (instruction.Parent != null)
                throw new ArgumentException("Instruction is already on a list");
            list.Add(instruction);
            Modifications++;
            instruction.Parent = this;
        }

        /// <summary>
        /// Add the given instruction at the specified index.
        /// </summary>
        public void Insert(int index, Instruction instruction)
        {
            if (instruction.Parent != null)
                throw new ArgumentException("Instruction is already on a list");
            list.Insert(index, instruction);
            Modifications++;
            instruction.Parent = this;
        }

        /// <summary>
        /// Insert all instructions of the given list into this list, starting at the given index.
        /// </summary>
        public void InsertFrom(int index, InstructionList range)
        {
            foreach (var ins in range)
            {
                list.Insert(index++, ins);
                ins.Parent = this;
            }
            Modifications++;
            range.list.Clear();
            range.Modifications++;
        }

        /// <summary>
        /// Remove the given instruction from this list.
        /// </summary>
        public void Remove(Instruction instruction)
        {
            if (instruction.Parent != this)
                throw new ArgumentException("Instruction is not on a list");
            list.Remove(instruction);
            Modifications++;
            instruction.Parent = null;
        }

        /// <summary>
        /// Remove the instruction at the given index from this list.
        /// </summary>
        public void RemoveAt(int index)
        {
            list[index].Parent = null;
            Modifications++;
            list.RemoveAt(index);
        }

        /// <summary>
        /// Gets the index of the given instruction in this list.
        /// </summary>
        /// <returns>-1 if not found</returns>
        public int IndexOf(Instruction instruction)
        {
            return list.IndexOf(instruction);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<Instruction> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// First instruction in the range.
        /// </summary>
        Instruction IInstructionRange.First
        {
            get { return (list.Count > 0) ? list[0] : null; }
        }

        /// <summary>
        /// Last (inclusive) instruction in the range.
        /// </summary>
        Instruction IInstructionRange.Last
        {
            get
            {
                var count = list.Count;
                return (count > 0) ? list[count - 1] : null;
            }
        }

        /// <summary>
        /// Get all instructions in the range.
        /// </summary>
        IEnumerable<Instruction> IInstructionRange.Instructions
        {
            get { return list; }
        }

        /// <summary>
        /// Create and add an instruction.
        /// </summary>
        public Instruction Add(ISourceLocation sequencePoint, RCode opcode, object operand, IEnumerable<Register> registers)
        {
            var instruction = new Instruction(opcode, operand, (registers != null) ? registers.ToArray() : null) { SequencePoint = sequencePoint };
            Add(instruction);
            return instruction;
        }

        /// <summary>
        /// Update the Index cache of all instructions.
        /// Doing this once if much faster then requesting the index of each individual instruction.
        /// </summary>
        public void UpdateIndexCache()
        {
            var count = list.Count;
            var modifications = Modifications;
            for (var index = 0; index < count; index++)
            {
                list[index].SetIndexCache(index, modifications);
            }
        }
    }
}
