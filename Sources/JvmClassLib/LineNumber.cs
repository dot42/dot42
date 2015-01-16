using System;

namespace Dot42.JvmClassLib
{
    public class LineNumber : IComparable<LineNumber>
    {
        public readonly int StartPC;
        public readonly int Number;

        public LineNumber(int startPc, int lineNumber)
        {
            StartPC = startPc;
            this.Number = lineNumber;
        }

        public int CompareTo(LineNumber other)
        {
            if (StartPC < other.StartPC) return -1;
            if (StartPC > other.StartPC) return 1;
            return 0;
        }
    }
}
