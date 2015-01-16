using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.Structure.Java
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET types.
    /// </summary>
    internal class StandardClassBuilder : ClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public StandardClassBuilder(AssemblyCompiler compiler, ClassFile typeDef)
            : base(compiler, typeDef)
        {
        }
    }
}
