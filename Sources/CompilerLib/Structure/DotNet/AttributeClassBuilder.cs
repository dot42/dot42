using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Mono.Cecil;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET attribute types.
    /// </summary>
    internal class AttributeClassBuilder : ClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public AttributeClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return 0; } }

        /// <summary>
        /// Implemented all fields and methods.
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            // Create normal members
            base.CreateMembers(targetPackage);

            // Create annotation interface and attribute build methods
            var mapping = AttributeAnnotationInterfaceBuilder.Create(null, Compiler, targetPackage, Type, Class);
            Compiler.Record(Type, mapping);

            // Add IAnnotationType annotation
            var annotationTypeRef = Compiler.GetDot42InternalType("IAnnotationType");
            var typeAnnotation = new Annotation { Type = annotationTypeRef.GetClassReference(targetPackage), Visibility = AnnotationVisibility.Runtime };
            typeAnnotation.Arguments.Add(new AnnotationArgument("AnnotationType", mapping.AnnotationInterfaceClass));
            Class.Annotations.Add(typeAnnotation);
        }
    }
}
