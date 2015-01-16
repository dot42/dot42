using System;
using System.Collections.Generic;
using Dot42.DexLib.Instructions;

namespace Dot42.CompilerLib.RL2DexCompiler
{
    partial class DebugInfoBuilder
    {
        /// <summary>
        /// Entry in the debug information.
        /// </summary>
        public abstract class Entry : IComparable<Entry>
        {
            public readonly int Offset;

            /// <summary>
            /// Default ctor
            /// </summary>
            protected Entry(int offset)
            {
                Offset = offset;
            }

            /// <summary>
            /// Sort priority (higher values come first)
            /// </summary>
            public abstract int Priority { get; }

            /// <summary>
            /// Generate debug info for this entry.
            /// </summary>
            public abstract void Generate(DebugInfo info, ref int lastLine, ref int lastAddress, ref string lastUrl, ref bool firstPositionEntry, HashSet<Register> startedVariables);

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public int CompareTo(Entry other)
            {
                if (Offset < other.Offset) return -1;
                if (Offset > other.Offset) return 1;

                var myPrio = Priority;
                var otherPrio = other.Priority;

                if (myPrio > otherPrio) return -1;
                if (myPrio < otherPrio) return 1;

                return 0;
            }
        }
    }
}
