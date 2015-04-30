using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ClassReferenceComparer : TypeReferenceComparer, IComparer<ClassReference>
    {
        public int Compare(ClassReference x, ClassReference y)
        {
            int comp = string.CompareOrdinal(x.Namespace, y.Namespace);
            if (comp != 0) return comp;

            return base.Compare(x, y);
        }
    }
}