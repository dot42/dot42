namespace Dot42.CompilerLib.RL.Extensions
{
    partial class RLExtensions
    {
        /// <summary>
        /// Is the given instruction contained in the given range?
        /// </summary>
        public static bool Contains(this IInstructionRange range, Instruction instruction)
        {
            if (instruction == null)
                return false;
            var f = range.First.Index;
            var l = range.Last.Index;
            var i = instruction.Index;

            return ((i >= f) && (i <= l));
        }

        /// <summary>
        /// Is the given range2 completly contained in the given range1?
        /// </summary>
        public static bool Contains(this IInstructionRange range1, IInstructionRange range2)
        {
            if (range2 == null)
                return false;

            var f1 = range1.First.Index;
            var l1 = range1.Last.Index;
            var f2 = range2.First.Index;
            var l2 = range2.Last.Index;

            return ((f2 >= f1) && (l2 <= l1));
        }

        /// <summary>
        /// Is there an intersection between range 1 and 2?
        /// </summary>
        public static bool IntersectsWith(this IInstructionRange range1, IInstructionRange range2)
        {
            if (range2 == null)
                return false;

            var f1 = range1.First.Index;
            var l1 = range1.Last.Index;
            var f2 = range2.First.Index;
            var l2 = range2.Last.Index;

            return
                ((f2 >= f1) && (f2 <= l1)) ||
                ((l2 >= f1) && (l2 <= l1)) ||
                ((f1 >= f2) && (f1 <= l2)) ||
                ((l1 >= f2) && (l1 <= l2));
            
            /*return range1.Contains(range2.First) || range1.Contains(range2.Last) ||
                   range2.Contains(range1.First) || range2.Contains(range1.Last);*/
        }

        /// <summary>
        /// Extend range 1 with range 2.
        /// </summary>
        public static IInstructionRange Extend(this IInstructionRange range1, IInstructionRange range2)
        {
            var first = range1.First;
            if (range2.First.Index < first.Index) first = range2.First;
            var last = range2.Last;
            if (range1.Last.Index > last.Index) last = range1.Last;
            return new InstructionRange(first, last);
        }
    }
}
