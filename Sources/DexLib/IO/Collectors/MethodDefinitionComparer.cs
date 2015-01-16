using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class MethodDefinitionComparer : MethodReferenceComparer, IComparer<MethodDefinition>
    {
        #region IComparer<MethodDefinition> Members

        public int Compare(MethodDefinition x, MethodDefinition y)
        {
            int result = x.IsVirtual.CompareTo(y.IsVirtual);

            if (result == 0)
                result = base.Compare(x, y);

            return result;
        }

        #endregion
    }
}