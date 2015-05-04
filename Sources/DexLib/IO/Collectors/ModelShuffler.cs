using System.Collections.Generic;
using Dot42.DexLib.Extensions;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ModelShuffler : BaseCollector<object>
    {
        public override void Collect(IList<ClassDefinition> classes)
        {
            classes.Shuffle();
            base.Collect(classes);
        }

        public override void Collect(IList<ClassReference> classes)
        {
            classes.Shuffle();
            base.Collect(classes);
        }

        public override void Collect(IList<MethodDefinition> methods)
        {
            methods.Shuffle();
            base.Collect(methods);
        }

        public override void Collect(IList<FieldDefinition> fields)
        {
            fields.Shuffle();
            base.Collect(fields);
        }

        public override void Collect(IList<Annotation> annotations)
        {
            annotations.Shuffle();
            base.Collect(annotations);
        }
    }
}