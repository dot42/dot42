using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ClassReferenceComparer : TypeReferenceComparer, IComparer<ClassReference>
    {
        #region IComparer<ClassReference> Members

        public int Compare(ClassReference x, ClassReference y)
        {
            return base.Compare(x, y);
        }

        #endregion
    }
}