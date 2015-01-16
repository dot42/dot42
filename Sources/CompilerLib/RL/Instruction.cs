using System;
using System.Text;
using Dot42.CompilerLib.RL.Extensions;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL
{
    public class Instruction : IComparable<Instruction>
    {
        private readonly RegisterList registers;
        private int? indexCache;
        private int indexCacheModifications;
        private InstructionList parent;

        public Instruction()
            : this(RCode.Nop, null, (Register[])null)
        {
        }

        public Instruction(RCode code, params Register[] registers)
            : this(code, null, registers)
        {
        }

        public Instruction(RCode code)
            : this(code, null, (Register[])null)
        {
        }

        public Instruction(RCode code, object operand)
            : this(code, operand, null)
        {
        }

        public Instruction(RCode code, object operand, Register[] registers)
        {
            Code = code;
            Operand = operand;
            this.registers = new RegisterList(registers);
        }

        public Instruction(Instruction source)
        {
            Code = source.Code;
            Operand = source.Operand;
            this.registers = new RegisterList(source.Registers);
            SequencePoint = source.SequencePoint;
        }

        public RCode Code { get; set; }
        public object Operand { get; set; }
        public RegisterList Registers { get { return registers; } }

        /// <summary>
        /// Does this register uses the given register?
        /// </summary>
        public bool Uses(Register r)
        {
            return registers.Contains(r);
        }

        /// <summary>
        /// Source code reference
        /// </summary>
        public ISourceLocation SequencePoint { get; set; }
        
        /// <summary>
        /// Gets the index of this instruction in the parent list.
        /// </summary>
        public int Index
        {
            get
            {
                var p = Parent;
                if (p != null)
                {
                    if (indexCache.HasValue && p.Modifications == indexCacheModifications)
                        return indexCache.Value;
                    indexCache = p.IndexOf(this);
                    indexCacheModifications = p.Modifications;
                    return indexCache.Value;
                }
                return -1;
            }
        }

        /// <summary>
        /// The list that contains this instruction
        /// </summary>
        internal InstructionList Parent
        {
            get { return parent; }
            set
            {
                indexCache = null;
                parent = value;
            }
        }

        /// <summary>
        /// Update the Index cache of all instructions.
        /// Doing this once if much faster then requesting the index of each individual instruction.
        /// </summary>
        internal void SetIndexCache(int index, int modifications)
        {
            indexCache = index;
            indexCacheModifications = modifications;
        }

        /// <summary>
        /// Gets the instruction that follows this instruction.
        /// </summary>
        public Instruction Next
        {
            get
            {
                var result = NextOrDefault;
                if (result == null)
                    throw new ArgumentException("There is no next instruction");
                return result;
            }
        }

        /// <summary>
        /// Gets the instruction that follows this instruction.
        /// If there is no next instruction, null is returned.
        /// </summary>
        public Instruction NextOrDefault
        {
            get
            {
                if (Parent == null)
                    throw new ArgumentNullException("Parent");
                var index = Index;
                if (index + 1 >= Parent.Count)
                    return null;
                return Parent[index + 1];
            }
        }

        /// <summary>
        /// Gets the instruction that preceeds this instruction.
        /// </summary>
        public Instruction Previous
        {
            get
            {
                var result = PreviousOrDefault;
                if (result == null)
                    throw new ArgumentException("There is no previous instruction");
                return result;
            }
        }

        /// <summary>
        /// Gets the instruction that preceeds this instruction.
        /// </summary>
        public Instruction PreviousOrDefault
        {
            get
            {
                if (Parent == null)
                    throw new ArgumentNullException("Parent");
                var index = Index;
                if (index == 0)
                    return null;
                return Parent[index - 1];
            }
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(Instruction other)
        {
            var x = Index;
            var y = other.Index;
            if (x < y) return -1;
            if (x > y) return 1;
            return 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Code.ToString());
            for (int i = 0; i < registers.Count; i++)
            {
                var r = registers[i];
                builder.AppendFormat(" {0}", r);
            }
            builder.Append(" ");
            if (Operand is Instruction)
            {
                builder.AppendFormat("-> D_{0:X4}", (Operand as Instruction).Index);
            }
            else if (Operand is string)
            {
                builder.Append(string.Concat("\"", Operand, "\""));
            }
            else if (Operand == null)
            {
                builder.Append("-");
            }
            else
            {
                builder.AppendFormat("[{0}] {1}", Operand.GetType().Name, Operand);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Change all references to the given old register with the given new register.
        /// </summary>
        public void ReplaceRegisterWith(Register oldRegister, Register newRegister)
        {
            registers.ReplaceRegisterWith(oldRegister, newRegister);
        }

        /// <summary>
        /// Is the register with the given index being assigned a value in this instruction?
        /// </summary>
        public bool IsDestinationRegister(int index)
        {
            var info = OpCodeInfo.Get(Code.ToDex());
            return ((info.GetUsage(index) & RegisterFlags.Destination) == RegisterFlags.Destination);
        }

    }
}
