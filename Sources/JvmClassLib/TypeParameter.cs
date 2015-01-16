using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Single argument of type signature.
    /// </summary>
    public sealed class TypeParameter
    {
        private readonly string name;
        private readonly TypeReference classBound;
        private readonly List<TypeReference> interfaceBound;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TypeParameter(string name, TypeReference classBound, IEnumerable<TypeReference> interfaceBound)
        {
            this.name = name;
            this.classBound = classBound;
            this.interfaceBound = (interfaceBound != null) ? interfaceBound.ToList() : new List<TypeReference>();
        }

        public string Name { get { return name; } }
        public TypeReference ClassBound { get { return classBound; } }
        public IEnumerable<TypeReference> InterfaceBound { get { return interfaceBound; } }

        public override string ToString()
        {
            return name;
        }
    }
}
