using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.JvmClassLib
{
    public sealed class MethodSignature : MethodDescriptor
    {
        private readonly string original;
        private readonly ReadOnlyCollection<TypeParameter> typeParameters;
        private readonly ReadOnlyCollection<TypeReference> throws;

        public MethodSignature(string original, IEnumerable<TypeParameter> typeParameters, TypeReference returnType,
                               IEnumerable<TypeReference> parameters, IEnumerable<TypeReference> throws) :
                                   base(returnType, parameters)
        {
            this.original = original;
            typeParameters = typeParameters ?? Enumerable.Empty<TypeParameter>();
            this.typeParameters = typeParameters.ToList().AsReadOnly();
            throws = throws ?? Enumerable.Empty<TypeReference>();
            this.throws = throws.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the original encoded signature
        /// </summary>
        public string Original { get { return original; } }

        /// <summary>
        /// Gets the type parameters
        /// </summary>
        public ReadOnlyCollection<TypeParameter> TypeParameters
        {
            get { return typeParameters; }
        }

        /// <summary>
        /// Gets the throws signatures
        /// </summary>
        public ReadOnlyCollection<TypeReference> Throws
        {
            get { return throws; }
        }
    }
}
