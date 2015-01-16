using System.Collections.Generic;

namespace Dot42.ImportJarLib.Model
{
    public sealed class NetModule
    {
        private readonly List<NetTypeDefinition> types = new List<NetTypeDefinition>();
        private readonly string scope;

        /// <summary>
        /// Default ctor
        /// </summary>
        public NetModule(string scope)
        {
            this.scope = scope;
        }


        /// <summary>
        /// All root level types
        /// </summary>
        public List<NetTypeDefinition> Types { get { return types; } }

        public string Scope
        {
            get { return scope; }
        }
    }
}
