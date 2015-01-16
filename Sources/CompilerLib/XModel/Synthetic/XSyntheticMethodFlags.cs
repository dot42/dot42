using System;

namespace Dot42.CompilerLib.XModel.Synthetic
{
    [Flags]
    public enum XSyntheticMethodFlags
    {
        Abstract = 0x0001,
        Virtual = 0x0002,
        Static = 0x0004,
        Constructor = 0x0008,
        Getter = 0x0010,
        Setter = 0x0020,
        Private = 0x0040,
        Protected = 0x0080,
    }
}
