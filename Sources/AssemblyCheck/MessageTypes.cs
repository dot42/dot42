using System;

namespace Dot42.AssemblyCheck
{
    internal enum MessageTypes
    {
        MissingType = 0,
        MissingMethod = 1,
        MissingField = 2,
        UnsupportedFeature = 3,
        General = 4,

        MissingFirst = MissingType,
        MissingLast = MissingField
    }
}
