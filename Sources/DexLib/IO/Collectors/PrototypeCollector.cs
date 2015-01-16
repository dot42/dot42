namespace Dot42.DexLib.IO.Collectors
{
    internal class PrototypeCollector : BaseCollector<Prototype>
    {
        public override void Collect(Prototype prototype)
        {
            base.Collect(prototype);

            // Override: Prototype .Equals & .GetHashCode 
            if (!Items.ContainsKey(prototype))
                Items[prototype.Clone()] = 0;

            Items[prototype]++;
        }
    }
}