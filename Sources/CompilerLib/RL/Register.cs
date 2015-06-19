using System;

namespace Dot42.CompilerLib.RL
{
    public class Register : IComparable<Register>
    {
        private readonly int index;
        private readonly RCategory category;
        private readonly RType type;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Register(int index, RCategory category, RType type)
        {
            this.index = index;
            this.category = category;
            this.type = type;
        }

        /// <summary>
        /// Gets the index of this register (this cannot be changed)
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the category of this register.
        /// </summary>
        public RCategory Category { get { return category; } }

        /// <summary>
        /// Type of data stored in this register
        /// </summary>
        public RType Type { get { return type; } }

        /// <summary>
        /// Flags indicating special requirements for this registry.
        /// </summary>
        public RFlags Flags { get; set; }

        /// <summary>
        /// Is the keep with next flag set?
        /// </summary>
        public RFlags KeepWith { get { return Flags & (RFlags.KeepWithNext|RFlags.KeepWithPrev); } }

        /// <summary>
        /// Is this register of category temp?
        /// </summary>
        public bool IsTemp { get { return (Category == RCategory.Temp); } }

        public bool PreventOptimization { get { return Category == RCategory.VariablePreventOptimization; } }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(Register other)
        {
            var x = index;
            var y = other.index;
            if (x < y) return -1;
            if (x > y) return 1;
            return 0;
        }

        public override string ToString()
        {
            switch (category)
            {
                case RCategory.Temp:
                    return "rt" + index;
                case RCategory.Variable:
                    return "rv" + index;
                case RCategory.VariablePreventOptimization:
                    return "rd" + index;
                case RCategory.Argument:
                    return "ra" + index;
                default:
                    throw new ArgumentException("Unknown category " + category);
            }
        }
    }
}
