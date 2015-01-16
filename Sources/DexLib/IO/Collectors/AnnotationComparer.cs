using System;
using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class AnnotationComparer : IComparer<Annotation>
    {
        private readonly ArgumentComparer argumentComparer = new ArgumentComparer();
        private readonly TypeReferenceComparer typeReferenceComparer = new TypeReferenceComparer();

        #region IComparer<Annotation> Members

        public int Compare(Annotation x, Annotation y)
        {
            int result = typeReferenceComparer.Compare(x.Type, y.Type);

            if (result == 0)
                result = x.Visibility.CompareTo(y.Visibility);

            if (result != 0)
                return result;

            for (int i = 0; i < Math.Min(x.Arguments.Count, y.Arguments.Count); i++)
            {
                result = argumentComparer.Compare(x.Arguments[i], y.Arguments[i]);
                if (result != 0)
                    return result;
            }

            if (x.Arguments.Count > y.Arguments.Count)
                return 1;

            if (y.Arguments.Count > x.Arguments.Count)
                return -1;

            return result;
        }

        #endregion
    }
}