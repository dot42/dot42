using Mono.Cecil;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Type system that considers the own module mscorlib.
    /// </summary>
    internal class AndroidTypeSystem : TypeSystem
    {
        private readonly ModuleDefinition module;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AndroidTypeSystem(ModuleDefinition module) : base(module)
        {
            this.module = module;
        }

        public override TypeReference LookupType(string @namespace, string name)
        {
            return module.GetType(@namespace, name);
        }
    }
}
