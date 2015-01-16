using System;

namespace Dot42.DebuggerLib
{
    partial class Jdwp
    {
        public enum SuspendPolicy
        {
            /// <summary>
            /// Suspend no threads when this event is encountered.  
            /// </summary>
            None = 0,

            /// <summary>
            /// Suspend the event thread when this event is encountered.  
            /// </summary>
            EventThread = 1,

            /// <summary>
            /// Suspend all threads when this event is encountered.  
            /// </summary>
            All = 2
        }
    }
}
