namespace Dot42.DexLib.IO.Collectors
{
    internal class TypeReferenceCollector : BaseCollector<TypeReference>
    {
        public override void Collect(ArrayType array)
        {
            // Do not 'over' collect String descriptors by iterating over array.ElementType
            Collect(array as TypeReference);
        }

        public override void Collect(TypeReference typeRef)
        {
            base.Collect(typeRef);
            if (typeRef != null)
            {
                if (!Items.ContainsKey(typeRef))
                    Items[typeRef] = 0;

                Items[typeRef]++;
            }
        }
    }
}