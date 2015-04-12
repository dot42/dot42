using System;
using System.Linq;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.DotNet
{
    partial class XBuilder
    {
        /// <summary>
        /// IL specific parameter.
        /// </summary>
        private sealed class ILGenericParameter : XGenericParameter
        {
            private readonly GenericParameter p;
            private IXGenericParameterProvider owner;
            private readonly Lazy<XTypeReference[]> constraints;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ILGenericParameter(XModule module, GenericParameter p) : 
                base(module)
            {
                this.p = p;
                constraints = new Lazy<XTypeReference[]>(()=>
                    p.Constraints.Select(k=>AsTypeReference(module, k)).ToArray());
            }

            /// <summary>
            /// Gets the owner of this generic parameter.
            /// </summary>
            public override IXGenericParameterProvider Owner
            {
                get
                {
                    return owner ?? (owner = (p.Type == GenericParameterType.Type) ? 
                        (IXGenericParameterProvider)AsTypeReference(Module, (TypeReference)p.Owner) :
                        AsMethodReference(Module, (MethodReference)p.Owner));
                }
            }

            /// <summary>
            /// Gets the index of this generic parameter in the owners list of generic parameters.
            /// </summary>
            public override int Position
            {
                get { return p.Position; }
            }

            public override XTypeReference[] Constraints { get { return constraints.Value; } }
        }
    }
}
