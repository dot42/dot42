using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class FieldReferenceComparer : IComparer<FieldReference>
    {
        private readonly StringComparer stringComparer = new StringComparer();
        private readonly TypeReferenceComparer typeReferenceComparer = new TypeReferenceComparer();

        #region IComparer<FieldReference> Members

        public int Compare(FieldReference x, FieldReference y)
        {
            int result = typeReferenceComparer.Compare(x.Owner, y.Owner);
            if (result == 0)
                result = stringComparer.Compare(x.Name, y.Name);
            return result;
        }

        #endregion
    }
}