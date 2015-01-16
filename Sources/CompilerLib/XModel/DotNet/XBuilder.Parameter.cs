using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.DotNet
{
    partial class XBuilder
    {
        /// <summary>
        /// IL specific parameter.
        /// </summary>
        private sealed class ILParameter : XParameter
        {
            private readonly XModule module;
            private readonly ParameterDefinition p;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ILParameter(XModule module, ParameterDefinition p) : 
                base(p.Name, GetKind(p))
            {
                this.module = module;
                this.p = p;
            }

            /// <summary>
            /// Gets the underlying parameter.
            /// </summary>
            public ParameterDefinition OriginalParameter { get { return p; } }

            /// <summary>
            /// Type of the parameter
            /// </summary>
            public override XTypeReference ParameterType
            {
                get { return AsTypeReference(module, p.ParameterType); }
            }

            private static XParameterKind GetKind(ParameterDefinition p)
            {
                if (p.IsIn && p.IsOut)
                    return XParameterKind.ByReference;
                if (p.IsOut) return XParameterKind.Output;
                return XParameterKind.Input;                
            }
        }
    }
}
