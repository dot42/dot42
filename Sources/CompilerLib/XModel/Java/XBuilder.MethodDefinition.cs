using System.Collections.ObjectModel;
using System.Linq;
using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific method definition.
        /// </summary>
        internal sealed class JavaMethodDefinition : XMethodDefinition
        {
            private readonly MethodDefinition method;
            private XTypeReference returnType;
            private ReadOnlyCollection<XParameter> parameters;
            private ReadOnlyCollection<XGenericParameter> genericParameters;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaMethodDefinition(XTypeDefinition declaringType, MethodDefinition method)
                : base(declaringType)
            {
                this.method = method;
            }

            public MethodDefinition OriginalMethod { get { return method; } }

            /// <summary>
            /// Does this method have the given name (or does the original method have this name).
            /// </summary>
            public override bool EqualsName(string name)
            {
                return (Name == name) || (method.Name == name);
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get
                {
                    var name = method.Name;
                    if (name == "<init>") return ".ctor";
                    if (name == "<clinit>") return ".cctor";
                    return method.Name;
                }
            }

            /// <summary>
            /// Is this an instance method ref?
            /// </summary>
            public override bool HasThis
            {
                get { return !method.IsStatic; }
            }

            /// <summary>
            /// Return type of the method
            /// </summary>
            public override XTypeReference ReturnType
            {
                get { return returnType ?? (returnType = AsTypeReference(Module, method.ReturnType, XTypeUsageFlags.ReturnType)); }
            }

            /// <summary>
            /// Parameters of the method
            /// </summary>
            public override ReadOnlyCollection<XParameter> Parameters
            {
                get { return parameters ?? (parameters = method.Parameters.Select((x, i) => new JavaParameter(Module, "p" + i, x)).Cast<XParameter>().ToList().AsReadOnly()); }
            }

            /// <summary>
            /// Gets all generic parameters
            /// </summary>
            public override ReadOnlyCollection<XGenericParameter> GenericParameters
            {
                get { return genericParameters ?? (genericParameters = method.Signature.TypeParameters.Select((x, i) => new XGenericParameter.SimpleXGenericParameter(this, i)).Cast<XGenericParameter>().ToList().AsReadOnly()); }
            }

            /// <summary>
            /// Is this method abstract?
            /// </summary>
            public override bool IsAbstract
            {
                get { return method.IsAbstract; }
            }

            /// <summary>
            /// Is this a virtual method?
            /// </summary>
            public override bool IsVirtual
            {
                get { return !(method.IsStatic || method.IsFinal); }
            }

            /// <summary>
            /// Is this a static method?
            /// </summary>
            public override bool IsStatic
            {
                get { return method.IsStatic; }
            }

            /// <summary>
            /// Is this an instance or class constructor?
            /// </summary>
            public override bool IsConstructor
            {
                get { return method.IsConstructor; }
            }

            /// <summary>
            /// Is this a property get method?
            /// </summary>
            public override bool IsGetter
            {
                get { return false; }
            }

            /// <summary>
            /// Is this a property set method?
            /// </summary>
            public override bool IsSetter
            {
                get { return false; }
            }

            /// <summary>
            /// Is this an android extension method?
            /// </summary>
            public override bool IsAndroidExtension
            {
                get { return false; }
            }

            /// <summary>
            /// Should this method be called with an invoke_direct in dex?
            /// </summary>
            public override bool IsDirect
            {
                get { return !method.IsStatic && (method.IsPrivate || method.IsConstructor); }
            }

            public override string ScopeId { get { return method.Descriptor; } }

            /// <summary>
            /// Should this method be called with invoke_interface?
            /// </summary>
            public override bool UseInvokeInterface
            {
                get { return method.DeclaringClass.IsInterface; }
            }

            /// <summary>
            /// Does this method need a parameter to pass the generic instance array for the generic types of the method itself?
            /// </summary>
            public override bool NeedsGenericInstanceMethodParameter
            {
                get { return false; }
            }

            /// <summary>
            /// Does this method have a DexNative attribute?
            /// </summary>
            public override bool HasDexNativeAttribute()
            {
                return false;
            }

            /// <summary>
            /// Does this method have a DexImport attribute?
            /// </summary>
            public override bool HasDexImportAttribute()
            {
                return false;
            }

            /// <summary>
            /// Try to get the names from the DexImport attribute attached to this method.
            /// </summary>
            public override bool TryGetDexImportNames(out string methodName, out string descriptor, out string className)
            {
                methodName = null;
                descriptor = null;
                className = null;
                return false;
            }

            /// <summary>
            /// Does this method have a JavaImport attribute?
            /// </summary>
            public override bool HasJavaImportAttribute()
            {
                return false;
            }

            /// <summary>
            /// Try to get the names from the JavaImport attribute attached to this method.
            /// </summary>
            public override bool TryGetJavaImportNames(out string methodName, out string descriptor, out string className)
            {
                methodName = null;
                descriptor = null;
                className = null;
                return false;
            }
        }
    }
}
