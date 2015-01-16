using System.Collections.Generic;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib.IO.Collectors
{
    internal class TypeReferenceComparer : IComparer<TypeReference>
    {
        public int Compare(TypeReference x, TypeReference y)
        {
            return string.CompareOrdinal(TypeDescriptor.Encode(x), TypeDescriptor.Encode(y));
        }
    }
}