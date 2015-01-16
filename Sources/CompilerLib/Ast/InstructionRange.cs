using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dot42.CompilerLib.Ast
{
    public sealed class InstructionRange
    {
        public readonly int From;
        private int to;   // Exlusive
		
        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="from">Start offset (inclusive)</param>
        /// <param name="to">End offset (exclusive)</param>
        public InstructionRange(int from, int to)
        {
            From = from;
            this.to = to;
        }

        /// <summary>
        /// Gets exlusive end offset.
        /// </summary>
        public int To { get { return to; } }

        public override string ToString()
        {
            return string.Format("{0}-{1}", From.ToString("X"), To.ToString("X"));
        }
		
        public static List<InstructionRange> OrderAndJoint(IEnumerable<InstructionRange> input)
        {
            if (input == null)
                throw new ArgumentNullException("Input is null!");
			
            var ranges = input.Where(r => r != null).OrderBy(r => r.From).ToList();
            for (int i = 0; i < ranges.Count - 1;) {
                var curr = ranges[i];
                var next = ranges[i + 1];
                // Merge consequtive ranges if they intersect
                if (curr.From <= next.From && next.From <= curr.To) {
                    curr.to = Math.Max(curr.To, next.To);
                    ranges.RemoveAt(i + 1);
                } else {
                    i++;
                }
            }
            return ranges;
        }
		
        public static IEnumerable<InstructionRange> Invert(IEnumerable<InstructionRange> input, int codeSize)
        {
            if (input == null)
                throw new ArgumentNullException("Input is null!");
			
            if (codeSize <= 0)
                throw new ArgumentException("Code size must be grater than 0");
			
            var ordered = OrderAndJoint(input);
            if (ordered.Count == 0) {
                yield return new InstructionRange(0, codeSize);
            } else {
                // Gap before the first element
                if (ordered.First().From != 0)
                    yield return new InstructionRange(0, ordered.First().From);
				
                // Gaps between elements
                for (int i = 0; i < ordered.Count - 1; i++)
                    yield return new InstructionRange(ordered[i].To, ordered[i + 1].From);
				
                // Gap after the last element
                Debug.Assert(ordered.Last().To <= codeSize);
                if (ordered.Last().To != codeSize)
                    yield return new InstructionRange(ordered.Last().To, codeSize);
            }
        }
    }
}