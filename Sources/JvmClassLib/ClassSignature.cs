using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.JvmClassLib
{
    public sealed class ClassSignature
    {
        private readonly ReadOnlyCollection<TypeParameter> typeParameters;
        private readonly string original;
        private readonly TypeReference superClass;
        private readonly ReadOnlyCollection<TypeReference> interfaces;

        public ClassSignature(string original, IEnumerable<TypeParameter> typeParameters, TypeReference superClass, IEnumerable<TypeReference> interfaces)
        {
            this.typeParameters = (typeParameters != null)
                                      ? typeParameters.ToList().AsReadOnly()
                                      : new ReadOnlyCollection<TypeParameter>(new List<TypeParameter>());
            this.original = original;
            this.superClass = superClass;
            this.interfaces = interfaces.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the original (encoded) signature.
        /// </summary>
        public string Original { get { return original; } }

        /// <summary>
        /// Gets the type parameters
        /// </summary>
        public ReadOnlyCollection<TypeParameter> TypeParameters { get { return typeParameters; } }

        /// <summary>
        /// Gets the super class signature
        /// </summary>
        public TypeReference SuperClass { get { return superClass; } }

        /// <summary>
        /// Gets the interfaces signatures
        /// </summary>
        public ReadOnlyCollection<TypeReference> Interfaces { get { return interfaces; } }
    }
}
