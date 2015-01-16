using System;

namespace Dot42.Ide.Descriptors
{
    [Flags]
    public enum AttributeFormat
    {
        Reference = 0x0001,
        String = 0x0002,
        Color = 0x0004, 
        Dimension = 0x0008,
        Boolean = 0x0010,
        Integer = 0x0020,
        Float = 0x0040,
        Fraction = 0x0080,
        Enum = 0x0100,
        Flag = 0x0200,
        Unknown = 0x10000000
    }
}
