using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.DexLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldReference = Mono.Cecil.FieldReference;
using MethodReference = Mono.Cecil.MethodReference;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET attribute types.
    /// </summary>
    internal class AttributeClassBuilder : ClassBuilder
    {
        private AttributeAnnotationInterface mapping;

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
        // Can be called very late, as nobody has dependencies on us.
        // This allows us to compile Dot42 in 'All' mode.
        protected override int SortPriority { get { return 200; } } 

        /// <summary>
        /// Implemented all fields and methods.
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            // Create normal members
            base.CreateMembers(targetPackage);

            if(!Type.IsAbstract)
            {
                // Create annotation interface and attribute build methods
                mapping = AttributeAnnotationInterfaceBuilder.Create(null, Compiler, targetPackage, Type, Class);
                Compiler.Record(Type, mapping);

                // Add IAnnotationType annotation
                var annotationTypeRef = Compiler.GetDot42InternalType("IAnnotationType");
                var typeAnnotation = new Annotation
                {
                    Type = annotationTypeRef.GetClassReference(targetPackage),
                    Visibility = AnnotationVisibility.Runtime
                };
                typeAnnotation.Arguments.Add(new AnnotationArgument("AnnotationType", mapping.AnnotationInterfaceClass));
                Class.Annotations.Add(typeAnnotation);
            }
        }

        protected override void FixUpMethods(DexTargetPackage targetPackage)
        {
            base.FixUpMethods(targetPackage);

            if (Type.IsAbstract) return;

            foreach (var fix in mapping.FixOperands)
            {
                var field = fix.Item2 as FieldReference;
                if (field != null)
                {
                    fix.Item1.Operand = field.GetReference(targetPackage, Compiler.Module);
                    continue;
                }

                var method = (MethodReference)fix.Item2;
                fix.Item1.Operand = method.GetReference(targetPackage, Compiler.Module);
            }
        }
    }
}
