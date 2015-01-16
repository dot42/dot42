using System;

namespace Dot42.DexLib.Instructions
{
    public sealed class Register : IComparable<Register>
    {
        private readonly int index;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Register(int index)
        {
            this.index = index;
        }

        /// <summary>
        /// 0-based index of this register.
        /// </summary>
        public int Index { get { return index; }}

        /// <summary>
        /// Does this register fit in 4 bits?
        /// </summary>
        public bool IsBits4 { get { return index < 16; } }

        /// <summary>
        /// Does this register fit in 8 bits?
        /// </summary>
        public bool IsBits8 { get { return index < 256; } }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(Register other)
        {
            var x = Index;
            var y = other.Index;
            if (x < y) return -1;
            if (x > y) return 1;
            return 0;
        }

        public override string ToString()
        {
            return string.Format("v{0}", Index);
        }
    }
}