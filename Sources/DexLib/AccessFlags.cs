using System;

namespace Dot42.DexLib
{
    [Flags]
    public enum AccessFlags : uint
    {
        Public = 0x1,
        Private = 0x2,
        Protected = 0x4,
        Static = 0x8,
        Final = 0x10,
        Synchronized = 0x20,
        Volatile = 0x40,
        Bridge = 0x40,
        Transient = 0x80,
        VarArgs = 0x80,
        Native = 0x100,
        Interface = 0x200,
        Abstract = 0x400,
        StrictFp = 0x800,
        Synthetic = 0x1000,
        Annotation = 0x2000,
        Enum = 0x4000,
        Unused = 0x8000,
        Constructor = 0x10000,
        DeclaredSynchronized = 0x20000
    }

    internal static class AccessFlagsExtensions
    {
        /// <summary>
        /// Create new access flags by including or excluding the given delta.
        /// </summary>
        internal static AccessFlags Set(this AccessFlags original, bool set, AccessFlags delta)
        {
            if (set)
            {
                return original | delta;
            }
            return original & ~delta;
        }
    }
}