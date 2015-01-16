using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Range of RL instructions
    /// </summary>
    [DebuggerDisplay("{@First}-{@Last} => {@Result}")]
    internal sealed class RLRange
    {
        private readonly Instruction first;
        private readonly Instruction last;
        private readonly RegisterSpec resultRegister;

        /// <summary>
        /// Default ctor
        /// </summary>
        public RLRange(IEnumerable<RLRange> prefix, Instruction first, Instruction last, RegisterSpec resultRegister)
        {
            var prefixList = (prefix != null) ? prefix.Where(x => x != null).ToList() : null;
            var firstInPrefix = (prefixList != null) ? prefixList.Select(x => x.First).FirstOrDefault() : null;
            var lastInPrefix = (prefixList != null) ? prefixList.Select(x => x.Last).LastOrDefault() : null;
            if (firstInPrefix != null)
                first = firstInPrefix;
            if (last == null)
                last = lastInPrefix;
            this.first = first;
            this.last = last;
            this.resultRegister = resultRegister;
        }

        
        /// <summary>
        /// Default ctor
        /// </summary>
        public RLRange(RLRange prefix, Instruction first, Instruction last, RegisterSpec resultRegister) 
        {
            if ((prefix != null) && (prefix.first != null)) first = prefix.first;
            if ((last == null) && (prefix != null)) last = prefix.last;
            this.first = first;
            this.last = last;
            this.resultRegister = resultRegister;
            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public RLRange(Instruction first, Instruction last, RegisterSpec resultRegister)
            : this((RLRange)null, first, last, resultRegister)
        {
        }

        /// <summary>
        /// No code, register only ctor
        /// </summary>
        public RLRange(IEnumerable<RLRange> prefix, RegisterSpec resultRegister)
            : this(prefix, null, null, resultRegister)
        {
        }

        /// <summary>
        /// No code, register only ctor
        /// </summary>
        public RLRange(RLRange prefix, RegisterSpec resultRegister)
            : this(prefix, null, null, resultRegister)
        {
        }

        /// <summary>
        /// No code, register only ctor
        /// </summary>
        public RLRange(RegisterSpec resultRegister)
            : this((RLRange)null, null, null, resultRegister)
        {
        }

        /// <summary>
        /// Single instruction range ctor
        /// </summary>
        public RLRange(IEnumerable<RLRange> prefix, Instruction single, RegisterSpec resultRegister)
            : this(prefix, single, single, resultRegister)
        {
        }

        /// <summary>
        /// Single instruction range ctor
        /// </summary>
        public RLRange(RLRange prefix, Instruction single, RegisterSpec resultRegister)
            : this(prefix, single, single, resultRegister)
        {
        }

        /// <summary>
        /// Single instruction range ctor
        /// </summary>
        public RLRange(Instruction single, RegisterSpec resultRegister)
            : this((RLRange)null, single, single, resultRegister)
        {
        }

        /// <summary>
        /// Gets the register that contains the result of the range 
        /// </summary>
        public RegisterSpec Result
        {
            get { return resultRegister; }
        }

        /// <summary>
        /// Gets the last instruction of the range
        /// </summary>
        public Instruction Last
        {
            get { return last; }
        }

        /// <summary>
        /// Gets the first instruction of the range.
        /// </summary>
        public Instruction First
        {
            get { return first; }
        }
    }

    /// <summary>
    /// RLRange extension methods
    /// </summary>
    internal static class DexRangeExtensions
    {
        /// <summary>
        /// Create a range that combined the given set or ordered ranges.
        /// </summary>
        public static RLRange Combine(this IEnumerable<RLRange> ranges)
        {
            if (ranges == null)
                return null;
            var list = ranges.Where(x => x != null).ToList();
            var count = list.Count;
            if (count == 0)
                return null;
            var last = list[count - 1];
            return new RLRange(list[0].First, last.Last, last.Result);
        }
    }
}
