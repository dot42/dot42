using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class FieldDefinitionComparer : FieldReferenceComparer, IComparer<FieldDefinition>
    {
        #region IComparer<FieldDefinition> Members

        public int Compare(FieldDefinition x, FieldDefinition y)
        {
            int result = y.IsStatic.CompareTo(x.IsStatic);

            if (result == 0)
                result = base.Compare(x, y);

            return result;
        }

        #endregion
    }
}