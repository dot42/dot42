using System.Collections.Generic;
using Dot42.DexLib.Extensions;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ModelShuffler : BaseCollector<object>
    {
        public override void Collect(List<ClassDefinition> classes)
        {
            classes.Shuffle();
            base.Collect(classes);
        }

        public override void Collect(List<ClassReference> classes)
        {
            classes.Shuffle();
            base.Collect(classes);
        }

        public override void Collect(List<MethodDefinition> methods)
        {
            methods.Shuffle();
            base.Collect(methods);
        }

        public override void Collect(List<FieldDefinition> fields)
        {
            fields.Shuffle();
            base.Collect(fields);
        }

        public override void Collect(List<Annotation> annotations)
        {
            annotations.Shuffle();
            base.Collect(annotations);
        }
    }
}