namespace Dot42.DexLib.IO.Collectors
{
    internal class MethodReferenceCollector : BaseCollector<MethodReference>
    {
        public override void Collect(MethodReference methodRef)
        {
            base.Collect(methodRef);
            if (methodRef != null)
            {
                if (!Items.ContainsKey(methodRef))
                    Items[methodRef] = 0;

                Items[methodRef]++;
            }
        }
    }
}