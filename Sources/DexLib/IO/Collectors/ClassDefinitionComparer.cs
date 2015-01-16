using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ClassDefinitionComparer : ClassReferenceComparer, IPartialComparer<ClassDefinition>,
                                             IComparer<ClassDefinition>
    {
        private readonly Dictionary<ClassDefinition, List<ClassDefinition>> dependenciesCache = new Dictionary<ClassDefinition, List<ClassDefinition>>();

        public int Compare(ClassDefinition x, ClassDefinition y)
        {
            return base.Compare(x, y);
        }

        public int? PartialCompare(ClassDefinition x, ClassDefinition y)
        {
            var xdependencies = CollectDependencies(x);
            var ydependencies = CollectDependencies(y);

            if (ydependencies.Contains(x))
            {
                if (xdependencies.Contains(y))
                    return 0;
                return -1;
            }

            if (xdependencies.Contains(y))
                return 1;

            return null;
        }

        private List<ClassDefinition> CollectDependencies(ClassDefinition cdef)
        {
            List<ClassDefinition> result;
            if (dependenciesCache.TryGetValue(cdef, out result))
                return result;

            var collector = new DependencyCollector();
            collector.Collect(cdef);
            result = collector.ToList();
            result.Remove(cdef);
            dependenciesCache[cdef] = result;
            return result;
        }
    }
}