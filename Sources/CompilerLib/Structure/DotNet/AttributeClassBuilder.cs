using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Mono.Cecil;
using FieldReference = Mono.Cecil.FieldReference;
using MethodReference = Mono.Cecil.MethodReference;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET attribute types.
    /// </summary>
    internal class AttributeClassBuilder : ClassBuilder
    {
        private AttributeAnnotationMapping mapping;

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
                mapping = AttributeAnnotationInstanceBuilder.CreateMapping(null, Compiler, targetPackage, Type, Class);
                Compiler.Record(Type, mapping);
            }
        }
    }
}
