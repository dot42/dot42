using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CecilExtensions
{
    /// <summary>
    /// Sequence of instructions.
    /// </summary>
    public sealed class ILSequence : IList<Instruction>
    {
        private readonly List<Instruction> instructions;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ILSequence() : this(32)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ILSequence(int capacity)
        {
            instructions = new List<Instruction>(capacity);
        }

        /// <summary>
        /// Gets the instruction as the given index.
        /// </summary>
        public Instruction this[int index]
        {
            get { return instructions[index]; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the number of instructions in this sequence.
        /// </summary>
        public int Length
        {
            get { return instructions.Count; }
        }

        /// <summary>
        /// Remove all
        /// </summary>
        public void Clear()
        {
            instructions.Clear();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            instructions.RemoveAt(index);
        }

        /// <summary>
        /// Gets the first instruction of the sequence.
        /// Throws an exception when there are no instructions.
        /// </summary>
        public Instruction First { get { return instructions[0]; } }

        /// <summary>
        /// Gets the last instruction of the sequence.
        /// Throws an exception when there are no instructions.
        /// </summary>
        public Instruction Last { get { return instructions[instructions.Count - 1]; } }

        public Instruction Emit(OpCode opcode)
        {
            var instruction = Instruction.Create(opcode);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, TypeReference type)
        {
            var instruction = Instruction.Create(opcode, type);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, MethodReference method)
        {
            var instruction = Instruction.Create(opcode, method);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, CallSite site)
        {
            var instruction = Instruction.Create(opcode, site);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, FieldReference field)
        {
            var instruction = Instruction.Create(opcode, field);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, string value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, byte value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, sbyte value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, int value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, long value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, float value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, double value)
        {
            var instruction = Instruction.Create(opcode, value);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, Instruction target)
        {
            var instruction = Instruction.Create(opcode, target);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, Instruction[] targets)
        {
            var instruction = Instruction.Create(opcode, targets);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, VariableDefinition variable)
        {
            var instruction = Instruction.Create(opcode, variable);
            instructions.Add(instruction);
            return instruction;
        }

        public Instruction Emit(OpCode opcode, ParameterDefinition parameter)
        {
            var instruction = Instruction.Create(opcode, parameter);
            instructions.Add(instruction);
            return instruction;
        }

        public void InsertBefore(Instruction target, Instruction instruction)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            var index = instructions.IndexOf(target);
            if (index == -1)
                throw new ArgumentOutOfRangeException("target");

            instructions.Insert(index, instruction);
        }

        public void InsertAfter(Instruction target, Instruction instruction)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            var index = instructions.IndexOf(target);
            if (index == -1)
                throw new ArgumentOutOfRangeException("target");

            instructions.Insert(index + 1, instruction);
        }

        public void Append(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            instructions.Add(instruction);
        }

        /// <summary>
        /// Add this sequence to the end of the given target collection.
        /// </summary>
        public void AppendTo(ILSequence target)
        {
            target.instructions.AddRange(instructions);
        }

        /// <summary>
        /// Add this sequence to the end of the given target list.
        /// </summary>
        public void AppendTo(List<Instruction> target)
        {
            target.AddRange(instructions);
        }

        /// <summary>
        /// Add this sequence to the end of the given target collection.
        /// </summary>
        public void AppendTo(ILProcessor target)
        {
            target.Append(instructions.ToArray());
        }

        /// <summary>
        /// Add this sequence to the end of the given target collection.
        /// </summary>
        public void AppendTo(MethodBody target)
        {
            AppendTo(target.GetILProcessor());
        }

        /// <summary>
        /// Insert this sequence to at the given index of the given target collection.
        /// </summary>
        public void InsertTo(int index, MethodBody target)
        {
            var targetInstructions = target.Instructions;
            var count = instructions.Count;
            for (var i = 0; i < count; i++)
            {
                targetInstructions.Insert(i + index, instructions[i]);
            }
        }

        /// <summary>
        /// Insert this sequence to at the given index of the given target collection.
        /// </summary>
        public void InsertTo(int index, IList<Instruction> targetInstructions)
        {
            var count = instructions.Count;
            for (var i = 0; i < count; i++)
            {
                targetInstructions.Insert(i + index, instructions[i]);
            }
        }

        /// <summary>
        /// Replace the original instruction with all instructions in this sequence.
        /// All references to the original instruction in exception handlers are extended to the next sequence.
        /// </summary>
        public void Replace(Instruction original, MethodBody target)
        {
            var index = target.Instructions.IndexOf(original);
            InsertTo(index + 1, target);
            /*if (target.HasExceptionHandlers)
            {
                // Re-route exception handlers
                foreach (var handler in target.ExceptionHandlers)
                {
                    if (handler.HandlerEnd == original)
                    {
                        handler.HandlerEnd = First;
                    }
                    if (handler.TryEnd == original)
                    {
                        handler.TryEnd = First;
                    }
                }
            }*/
            // Replace original instruction with nop. 
            // This way all jump targets remain in place.
            original.OpCode = OpCodes.Nop;
            original.Operand = null;
        }

        /// <summary>
        /// Insert this sequence to at the given index of the given target collection.
        /// </summary>
        public void InsertTo(int index, ILSequence target)
        {
            target.instructions.InsertRange(index, instructions);
        }

        /// <summary>
        /// Insert this sequence just before the given instruction in of the given target collection.
        /// </summary>
        public void InsertToBefore(Instruction next, ILSequence target)
        {
            InsertTo(target.IndexOf(next), target);
        }

        /// <summary>
        /// Insert this sequence just before the given instruction in of the given target collection.
        /// </summary>
        public void InsertToBefore(Instruction next, ILProcessor target)
        {
            target.InsertBefore(next, instructions.ToArray());
        }

        /// <summary>
        /// Insert this sequence just before the given instruction in of the given target collection.
        /// </summary>
        public void InsertToBefore(Instruction next, MethodBody target)
        {
            var index = target.Instructions.IndexOf(next);
            InsertTo(index - 1, target);
        }

        /// <summary>
        /// Insert this sequence just after the given instruction in of the given target collection.
        /// </summary>
        public void InsertToAfter(Instruction prev, ILSequence target)
        {
            InsertTo(target.IndexOf(prev) + 1, target);
        }

        /// <summary>
        /// Insert this sequence just after the given instruction in of the given target collection.
        /// </summary>
        public void InsertToAfter(Instruction prev, ILProcessor target)
        {
            target.InsertAfter(prev, instructions.ToArray());
        }

        /// <summary>
        /// Insert this sequence just after the given instruction in of the given target collection.
        /// </summary>
        public void InsertToAfter(Instruction prev, MethodBody target)
        {
            var index = target.Instructions.IndexOf(prev);
            InsertTo(index + 1, target);
        }

        /// <summary>
        /// Gets the index of the given instruction in this sequence (0..)
        /// </summary>
        public int IndexOf(Instruction instruction)
        {
            return instructions.IndexOf(instruction);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        void IList<Instruction>.Insert(int index, Instruction item)
        {
            throw new NotImplementedException();
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
            return instructions.GetEnumerator();
        }

        /// <summary>
        /// Convert to human readable string
        /// </summary>
        public override string ToString()
        {
            return string.Join(Environment.NewLine, instructions.Select(x => x.ToString()));
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
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        void ICollection<Instruction>.Add(Instruction item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        bool ICollection<Instruction>.Contains(Instruction item)
        {
            return instructions.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        void ICollection<Instruction>.CopyTo(Instruction[] array, int arrayIndex)
        {
            instructions.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        bool ICollection<Instruction>.Remove(Instruction item)
        {
            return instructions.Remove(item);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        int ICollection<Instruction>.Count
        {
            get { return instructions.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool ICollection<Instruction>.IsReadOnly
        {
            get { return false; }
        }
    }
}
