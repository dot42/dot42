using Mono.Cecil;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET types.
    /// </summary>
    internal class StandardClassBuilder : ClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public StandardClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return 0; } }
    }
}
