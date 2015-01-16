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
            Collect(@class.InnerClasses);
            Collect(@class.Interfaces);
            Collect(@class.SuperClass);
            Collect(@class as ClassReference);
        }
    }
}