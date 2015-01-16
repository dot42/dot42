using System;

namespace Dot42.CompilerLib.XModel.Synthetic
{
    [Flags]
    public enum XSyntheticTypeFlags
    {
        Static = 0x0001,
        Private = 0x0002,
        Protected = 0x004,
        ValueType = 0x0008,
        Interface = 0x0010,
        Abstract = 0x0020,
        Sealed = 0x0040,
    }
}
