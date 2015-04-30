
using System.Collections.Generic;
using System.Linq;

namespace Dot42.DexLib.IO.Collectors
{
    internal class DependencyCollector : BaseCollector<ClassDefinition>
    {
        public override void Collect(TypeReference tref)
        {
            if (tref is ClassDefinition)
            {
                var @class = tref as ClassDefinition;
                if (!Items.ContainsKey(@class))
                    Items.Add(@class, 0);

                Items[@class]++;
            }
        }

        public override void Collect(ClassDefinition @class)
        {
            if(@class.Owner != null)
                Collect(@class.Owner);
            Collect(@class.Interfaces);
            Collect(@class.SuperClass);
            Collect(@class as ClassReference);
        }

        public IEnumerable<ClassDefinition> GetDependencies(ClassDefinition @class)
        {
            Collect(@class);
            // ReSharper disable once PossibleUnintendedReferenceComparison
            return ToList().Where(cdef => cdef != @class);
        }
    }
}