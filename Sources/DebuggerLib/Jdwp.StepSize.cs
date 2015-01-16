using System;

namespace Dot42.DebuggerLib
{
    partial class Jdwp
    {
        public enum StepSize
        {
            /// <summary>
            /// Step by the minimum possible amount (often a bytecode instruction).  
            /// </summary>
            Minimum = 0,

            /// <summary>
            /// Step to the next source line unless there is no line number information in which case a MIN step is done instead.  
            /// </summary>
            Line = 1
        }
    }
}
