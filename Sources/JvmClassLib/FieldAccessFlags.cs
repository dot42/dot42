using System;

namespace Dot42.JvmClassLib
{
    [Flags]
    public enum FieldAccessFlags
    {
        Public = 0x0001,
        Private = 0x0002,
        Protected = 0x0004,
        Static = 0x0008,
        Final = 0x0010,
        Volatile = 0x0040,
        Transient = 0x0080,
        Synthetic = 0x1000,
        Enum = 0x4000,

        AccessMask = 0x0007,
    }
}
