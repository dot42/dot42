using System.Collections.ObjectModel;
using System.Linq;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.DotNet
{
    partial class XBuilder
    {
        /// <summary>
        /// IL specific method reference.
        /// </summary>
        internal sealed class ILMethodReference : XMethodReference
        {
            private readonly MethodReference method;
            //private XTypeReference returnType;
            private ReadOnlyCollection<XParameter> parameters;
            private ReadOnlyCollection<XGenericParameter> genericParameters;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ILMethodReference(XTypeReference declaringType, MethodReference method)
                : base(declaringType)
            {
                this.method = method;
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return method.Name + CreateSignPostfix(method); }
            }

            /// <summary>
            /// Is this an instance method ref?
            /// </summary>
            public override bool HasThis
            {
                get { return method.HasThis; }
            }

            /// <summary>
            /// Return type of the method
            /// </summary>
            public override XTypeReference ReturnType
            {
                //get { return returnType ?? (returnType = AsTypeReference(Module, method.ReturnType)); }
                get { return AsTypeReference(Module, method.ReturnType); }
            }

            /// <summary>
            /// Parameters of the method
            /// </summary>
            public override ReadOnlyCollection<XParameter> Parameters
            {
                get { return parameters ?? (parameters = method.Parameters.Select((x, i) => new ILParameter(Module, x)).Cast<XParameter>().ToList().AsReadOnly()); }
            }

            /// <summary>
            /// Gets all generic parameters
            /// </summary>
            public override ReadOnlyCollection<XGenericParameter> GenericParameters
            {
                get { return genericParameters ?? (genericParameters = method.GenericParameters.Select((x, i) => new XGenericParameter.SimpleXGenericParameter(this, i)).Cast<XGenericParameter>().ToList().AsReadOnly()); }
            }
        }
    }
}
