using System;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// Flags indicating special requirements on registers.
    /// </summary>
    [Flags]
    public enum RFlags
    {
        None = 0,

        /// <summary>
        /// Index of next register must be my index + 1
        /// </summary>
        KeepWithNext = 0x01,

        /// <summary>
        /// Index of prev register must be my index - 1
        /// </summary>
        KeepWithPrev = 0x02
    }
}