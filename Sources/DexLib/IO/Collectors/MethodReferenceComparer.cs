using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class MethodReferenceComparer : IComparer<MethodReference>
    {
        private readonly PrototypeComparer prototypeComparer = new PrototypeComparer();
        private readonly StringComparer stringComparer = new StringComparer();
        private readonly TypeReferenceComparer typeReferenceComparer = new TypeReferenceComparer();

        #region IComparer<MethodReference> Members

        public int Compare(MethodReference x, MethodReference y)
        {
            int result = typeReferenceComparer.Compare(x.Owner, y.Owner);

            if (result == 0)
                result = stringComparer.Compare(x.Name, y.Name);

            if (result == 0)
                result = prototypeComparer.Compare(x.Prototype, y.Prototype);

            return result;
        }

        #endregion
    }
}