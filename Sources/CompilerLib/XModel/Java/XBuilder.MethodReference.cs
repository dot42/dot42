using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific method reference.
        /// </summary>
        internal sealed class JavaMethodReference : XMethodReference
        {
            private readonly string name;
            private readonly bool hasThis;
            private readonly XTypeReference returnType;
            private readonly string javaName;
            private readonly string javaDescriptor;
            private readonly string javaClassName;
            private readonly ReadOnlyCollection<XParameter> parameters;
            private readonly ReadOnlyCollection<XGenericParameter> genericParameters;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaMethodReference(string name, bool hasThis, XTypeReference returnType, XTypeReference declaringType,
                          IEnumerable<XParameter> parameters, IEnumerable<string> genericParameterNames, string javaName, string javaDescriptor, string javaClassName)
                : base(declaringType)
            {
                this.name = name;
                this.hasThis = hasThis;
                this.returnType = returnType;
                this.javaName = javaName;
                this.javaDescriptor = javaDescriptor;
                this.javaClassName = javaClassName;
                this.parameters = (parameters ?? Enumerable.Empty<XParameter>()).ToList().AsReadOnly();
                genericParameters = (genericParameterNames ?? Enumerable.Empty<string>()).Select((x, i) => new XGenericParameter.SimpleXGenericParameter(this, i)).Cast<XGenericParameter>().ToList().AsReadOnly();
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return name; }
            }

            public string JavaName { get { return javaName; } }
            public string JavaDecriptor { get { return javaDescriptor; } }
            public string JavaClassName { get { return javaClassName; } }

            /// <summary>
            /// Is this an instance method ref?
            /// </summary>
            public override bool HasThis
            {
                get { return hasThis; }
            }

            /// <summary>
            /// Gets all generic parameters
            /// </summary>
            public override ReadOnlyCollection<XGenericParameter> GenericParameters
            {
                get { return genericParameters; }
            }

            /// <summary>
            /// Return type of the method
            /// </summary>
            public override XTypeReference ReturnType
            {
                get { return returnType; }
            }

            /// <summary>
            /// Parameters of the method
            /// </summary>
            public override ReadOnlyCollection<XParameter> Parameters
            {
                get { return parameters; }
            }

            /// <summary>
            /// Resolve this reference to it's definition.
            /// </summary>
            public override bool TryResolve(out XMethodDefinition method)
            {
                method = null;
                var declaringTypeRef = DeclaringType.IsArray ? Module.TypeSystem.Object : DeclaringType.GetElementType();
                XTypeDefinition declaringType;
                if (!declaringTypeRef.TryResolve(out declaringType))
                    return false;
                return declaringType.TryGet(this, out method);
            }
        }
    }
}
