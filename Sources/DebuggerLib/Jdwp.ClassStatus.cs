using System;

namespace Dot42.DebuggerLib
{
    partial class Jdwp
    {
        [Flags]
        public enum ClassStatus
        {
            Verified = 1,
            Prepared = 2,
            Initialized = 4,
            Error = 8
        }
    }
}
