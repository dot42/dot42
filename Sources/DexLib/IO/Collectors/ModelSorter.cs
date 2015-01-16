using System.Collections.Generic;

namespace Dot42.DexLib.IO.Collectors
{
    internal class ModelSorter : BaseCollector<object>
    {
        private readonly AnnotationComparer ac;
        private readonly ClassDefinitionComparer cdefc;
        private readonly ClassReferenceComparer crefc;
        private readonly FieldDefinitionComparer fdefc;
        private readonly MethodDefinitionComparer mdefc;

        public ModelSorter()
        {
            cdefc = new ClassDefinitionComparer();
            crefc = new ClassReferenceComparer();
            mdefc = new MethodDefinitionComparer();
            fdefc = new FieldDefinitionComparer();
            ac = new AnnotationComparer();
        }

        public override void Collect(List<ClassDefinition> classes)
        {
            classes.Sort(cdefc);
            base.Collect(classes);
        }

        public override void Collect(List<ClassReference> classes)
        {
            classes.Sort(crefc);
            base.Collect(classes);
        }

        public override void Collect(List<MethodDefinition> methods)
        {
            methods.Sort(mdefc);
            base.Collect(methods);
        }

        public override void Collect(List<FieldDefinition> fields)
        {
            fields.Sort(fdefc);
            base.Collect(fields);
        }

        public override void Collect(List<Annotation> annotations)
        {
            annotations.Sort(ac);
            base.Collect(annotations);
        }
    }
}