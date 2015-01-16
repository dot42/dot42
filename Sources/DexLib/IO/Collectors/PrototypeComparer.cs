using System;
using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class PrototypeComparer : IComparer<Prototype>
    {
        private readonly TypeReferenceComparer typeReferenceComparer = new TypeReferenceComparer();

        #region IComparer<Prototype> Members

        public int Compare(Prototype x, Prototype y)
        {
            int crt = typeReferenceComparer.Compare(x.ReturnType, y.ReturnType);
            if (crt == 0)
            {
                if (x.Parameters.Count == 0 && y.Parameters.Count != 0)
                    return -1;

                if (y.Parameters.Count == 0 && x.Parameters.Count != 0)
                    return 1;

                int minp = Math.Min(x.Parameters.Count, y.Parameters.Count);
                for (int i = 0; i < minp; i++)
                {
                    int cp = typeReferenceComparer.Compare(x.Parameters[i].Type, y.Parameters[i].Type);
                    if (cp != 0)
                        return cp;
                }
                return x.Parameters.Count.CompareTo(y.Parameters.Count);
            }
            return crt;
        }

        #endregion
    }
}