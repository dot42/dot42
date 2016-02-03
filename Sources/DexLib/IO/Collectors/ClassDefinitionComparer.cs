using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ClassDefinitionComparer : ClassReferenceComparer, IComparer<ClassDefinition>
    {
        public static readonly ClassDefinitionComparer Default = new ClassDefinitionComparer();

        public int Compare(ClassDefinition x, ClassDefinition y)
        {
            return base.Compare(x, y);
        }
    }
}