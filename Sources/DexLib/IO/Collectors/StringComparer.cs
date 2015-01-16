using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class StringComparer : IComparer<string>
    {
        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            return string.CompareOrdinal(x, y);
        }

        #endregion
    }
}