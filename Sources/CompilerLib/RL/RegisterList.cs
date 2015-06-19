using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Optimized list of registers
    /// </summary>
    public class RegisterList : IEnumerable<Register>
    {
        private readonly List<Register> registers;
        private int minIndex;
        private int maxIndex;

        internal RegisterList(IEnumerable<Register> registers)
        {
            this.registers = (registers != null) ? registers.ToList() : new List<Register>();
            minIndex = (this.registers.Count == 0) ? int.MaxValue : this.registers.Min(x => x.Index);
            maxIndex = (this.registers.Count == 0) ? int.MinValue : this.registers.Max(x => x.Index);
        }

        /// <summary>
        /// Gets the number of registers.
        /// </summary>
        public int Count
        {
            get { return registers.Count; }
        }

        /// <summary>
        /// Gets/sets a register at the given index.
        /// </summary>
        public Register this[int index]
        {
            get { return registers[index]; }
            set
            {
                registers[index] = value;
                minIndex = Math.Min(minIndex, value.Index);
                maxIndex = Math.Max(maxIndex, value.Index);
            }
        }

        /// <summary>
        /// Add the given register to this list.
        /// </summary>
        public void Add(Register value)
        {
            registers.Add(value);
            minIndex = Math.Min(minIndex, value.Index);
            maxIndex = Math.Max(maxIndex, value.Index);            
        }

        /// <summary>
        /// Remove all entries
        /// </summary>
        public void Clear()
        {
            registers.Clear();
            minIndex = int.MaxValue;
            maxIndex = int.MinValue;
        }

        /// <summary>
        /// Does this list contain the given register?
        /// </summary>
        public bool Contains(Register r)
        {
            var rIndex = r.Index;
            if ((rIndex < minIndex) || (rIndex > maxIndex))
                return false;
            return registers.Contains(r);
        }

        /// <summary>
        /// Gets the index of the given register is this list.
        /// </summary>
        public int IndexOf(Register r)
        {
            var rIndex = r.Index;
            if ((rIndex < minIndex) || (rIndex > maxIndex))
                return -1;
            return registers.IndexOf(r);
        }

        /// <summary>
        /// Change all references to the given old register with the given new register.
        /// </summary>
        public void ReplaceRegisterWith(Register oldRegister, Register newRegister)
        {
            var rIndex = oldRegister.Index;
            if ((rIndex < minIndex) || (rIndex > maxIndex))
                return; // Not in here

            var count = registers.Count;
            var replaced = false;
            for (var i = 0; i < count; i++)
            {
                if (registers[i] == oldRegister)
                {
                    if (oldRegister.KeepWith != newRegister.KeepWith)
                        throw new ArgumentException("New register has different keep-with-next value");
                    registers[i] = newRegister;
                    replaced = true;
                }
            }

            if (replaced)
            {
                minIndex = Math.Min(minIndex, newRegister.Index);
                maxIndex = Math.Max(maxIndex, newRegister.Index);
            }
        }

        public IEnumerator<Register> GetEnumerator()
        {
            return registers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
