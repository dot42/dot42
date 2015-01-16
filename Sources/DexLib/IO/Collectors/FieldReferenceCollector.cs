namespace Dot42.DexLib.IO.Collectors
{
    internal class FieldReferenceCollector : BaseCollector<FieldReference>
    {
        public override void Collect(FieldReference fieldRef)
        {
            base.Collect(fieldRef);
            if (fieldRef != null)
            {
                if (!Items.ContainsKey(fieldRef))
                    Items[fieldRef] = 0;

                Items[fieldRef]++;
            }
        }
    }
}