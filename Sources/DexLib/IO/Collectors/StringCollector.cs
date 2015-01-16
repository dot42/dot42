using System;
using Dot42.DexLib.Instructions;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib.IO.Collectors
{
    internal class StringCollector : BaseCollector<String>
    {
        public override void Collect(DebugInfo debugInfo)
        {
            base.Collect(debugInfo);

            if (debugInfo != null && debugInfo.Owner != null && debugInfo.Owner.Owner != null &&
                !debugInfo.Owner.Owner.IsStatic)
                Collect("this");
        }

        public override void Collect(Prototype prototype)
        {
            base.Collect(prototype);

            // Shorty descriptor
            Collect(TypeDescriptor.Encode(prototype));
        }

        public override void Collect(ArrayType array)
        {
            // Do not 'over' collect String descriptors by iterating over array.ElementType
            Collect(array as TypeReference);
        }

        public override void Collect(TypeReference tref)
        {
            base.Collect(tref);
            Collect(TypeDescriptor.Encode(tref));
        }

        public override void Collect(string str)
        {
            base.Collect(str);

            if (str != null)
            {
                if (!Items.ContainsKey(str))
                    Items[str] = 0;

                Items[str]++;
            }
        }
    }
}