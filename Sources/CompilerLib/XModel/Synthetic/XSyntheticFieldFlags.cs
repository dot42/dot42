using System;

namespace Dot42.CompilerLib.XModel.Synthetic
{
    [Flags]
    public enum XSyntheticFieldFlags
    {
        Static = 0x0001,
        Private = 0x0002,
        Protected = 0x004,
        ReadOnly = 0x0008,
        Public = 0x0010,
    }
}
