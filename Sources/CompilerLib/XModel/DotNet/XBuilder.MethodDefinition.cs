using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.DotNet
{
    partial class XBuilder
    {
        /// <summary>
        /// IL specific method definition.
        /// </summary>
        internal sealed class ILMethodDefinition : XMethodDefinition
        {
            private readonly MethodDefinition method;
            //private XTypeReference returnType;
            private ReadOnlyCollection<XParameter> parameters;
            private ReadOnlyCollection<XGenericParameter> genericParameters;
            private string dexImportName;
            private string javaImportName;
            private bool? useInvokeInterface;
            private bool? needsGenericInstanceMethodParameter;
            private string scopeId;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ILMethodDefinition(ILTypeDefinition declaringType, MethodDefinition method, string forceScopeId=null)
                : base(declaringType)
            {
                this.method = method;
                OriginalReturnType = method.ReturnType;
                scopeId = forceScopeId;
            }

            public MethodDefinition OriginalMethod { get { return method; } }

            /// <summary>
            /// Does this method have the given name (or does the original method have this name).
            /// </summary>
            public override bool EqualsName(string name)
            {
                return Name == name || method.Name == name || method.OriginalName == name;
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return method.Name; }
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
                get { return method.IsVirtual; }
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
                get { return method.IsGetter; }
            }

            /// <summary>
            /// Is this a property set method?
            /// </summary>
            public override bool IsSetter
            {
                get { return method.IsSetter; }
            }

            /// <summary>
            /// Is this an android extension method?
            /// </summary>
            public override bool IsAndroidExtension
            {
                get { return method.IsAndroidExtension(); }
            }

            /// <summary>
            /// Should this method be called with an invoke_direct in dex?
            /// </summary>
            public override bool IsDirect
            {
                get { return method.IsDirect(); }
            }

            /// <summary>
            /// our unique scope id
            /// </summary>
            public override string ScopeId
            {
                get
                {
                    return scopeId ?? (scopeId = GetScopeId());
                }
            }

            /// <summary>
            /// Gets the "base" method of the given method.
            /// </summary>
            public override XMethodDefinition GetBaseMethod(bool ignoreVirtual)
            {
                // Try via IL first
                var ilBaseMethod = method.GetBaseMethod();
                if (ilBaseMethod != null)
                {
                    return AsMethodDefinition(Module, ilBaseMethod);
                }
                return base.GetBaseMethod(ignoreVirtual);
            }

            /// <summary>
            /// Does this method need a parameter to pass the generic instance array for the generic types of the method itself?
            /// </summary>
            public override bool NeedsGenericInstanceMethodParameter
            {
                get
                {
                    if (!needsGenericInstanceMethodParameter.HasValue)
                    {
                        needsGenericInstanceMethodParameter = (method.GenericParameters.Any() && !method.HasDexImportAttribute());
                    }
                    return needsGenericInstanceMethodParameter.Value;
                }
            }

            /// <summary>
            /// Should this method be called with invoke_interface?
            /// </summary>
            public override bool UseInvokeInterface
            {
                get
                {
                    if (!useInvokeInterface.HasValue)
                    {
                        useInvokeInterface = DoUseInvokeInterface();
                    }
                    return useInvokeInterface.Value;
                }
            }

            public TypeReference OriginalReturnType { get; set; }

            /// <summary>
            /// Should this method be called with invoke_interface?
            /// </summary>
            private bool DoUseInvokeInterface()
            {
                if (method.DeclaringType.IsInterface)
                    return true;
                string methodName;
                string descriptor;
                string className;
                if (TryGetDexImportNames(out methodName, out descriptor, out className))
                {
                    var typeRef = Java.XBuilder.AsTypeReference(Module, className, XTypeUsageFlags.DeclaringType);
                    XTypeDefinition typeDef;
                    if (typeRef.TryResolve(out typeDef) && typeDef.IsInterface)
                    {
                        return true;
                    }
                }
                else if (TryGetJavaImportNames(out methodName, out descriptor, out className))
                {
                    var typeRef = Java.XBuilder.AsTypeReference(Module, className, XTypeUsageFlags.DeclaringType);
                    XTypeDefinition typeDef;
                    if (typeRef.TryResolve(out typeDef) && typeDef.IsInterface)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Is this reference equal to the given other reference?
            /// </summary>
            public override bool IsSameExceptDeclaringType(XMethodReference other)
            {
                if (base.IsSameExceptDeclaringType(other))
                    return true;
                EnsureDexImportName();
                // Check against dex import 
                if ((dexImportName == other.Name) && (other.Parameters.Count == Parameters.Count) && (other.HasThis == HasThis))
                {
                    var descriptor = CreateNoGenericsDescriptor(this);
                    var otherDescriptor = CreateNoGenericsDescriptor(other);
                    return (descriptor == otherDescriptor);
                }
                EnsureJavaImportName();
                // Check against java import 
                if ((javaImportName == other.Name) && (other.Parameters.Count == Parameters.Count) && (other.HasThis == HasThis))
                {
                    var descriptor = CreateNoGenericsDescriptor(this);
                    var otherDescriptor = CreateNoGenericsDescriptor(other);
                    return (descriptor == otherDescriptor);
                }
                return false;
            }

            /// <summary>
            /// Ensure dexImportName is loaded.
            /// </summary>
            private void EnsureDexImportName()
            {
                if (dexImportName == null)
                {
                    string methodName;
                    string descriptor;
                    string className;
                    if (TryGetDexImportNames(out methodName, out descriptor, out className))
                    {
                        dexImportName = methodName;
                        if (dexImportName == "<init>") dexImportName = ".ctor";
                        else if (dexImportName == "<clinit>") dexImportName = ".cctor";
                    }
                    else
                    {
                        dexImportName = "<none>";
                    }
                }
            }

            /// <summary>
            /// Ensure javaImportName is loaded.
            /// </summary>
            private void EnsureJavaImportName()
            {
                if (javaImportName == null)
                {
                    string methodName;
                    string descriptor;
                    string className;
                    if (TryGetJavaImportNames(out methodName, out descriptor, out className))
                    {
                        javaImportName = methodName;
                        if (javaImportName == "<init>") javaImportName = ".ctor";
                        else if (javaImportName == "<clinit>") javaImportName = ".cctor";
                    }
                    else
                    {
                        javaImportName = "<none>";
                    }
                }
            }

            /// <summary>
            /// Create a descriptor for comparing the given method without generics.
            /// </summary>
            private static string CreateNoGenericsDescriptor(XMethodReference method)
            {
                return /*CreateNoGenericsDescriptor(method.ReturnType) + ":" +*/
                       string.Join(",", method.Parameters.Select(x => XBuilder.CreateNoGenericsDescriptor(x.ParameterType)));
            }

            /// <summary>
            /// Does this method have a DexNative attribute?
            /// </summary>
            public override bool HasDexNativeAttribute()
            {
                return method.HasDexNativeAttribute();
            }

            /// <summary>
            /// Does this method have a DexImport attribute?
            /// </summary>
            public override bool HasDexImportAttribute()
            {
                return method.HasDexImportAttribute();
            }

            /// <summary>
            /// Try to get the names from the DexImport attribute attached to this method.
            /// </summary>
            public override bool TryGetDexImportNames(out string methodName, out string descriptor, out string className)
            {
                var attr = method.GetDexImportAttribute();
                if (attr == null)
                {
                    methodName = null;
                    descriptor = null;
                    className = null;
                    return false;
                }
                attr.GetDexOrJavaImportNames(method, out methodName, out descriptor, out className);
                return true;
            }

            /// <summary>
            /// Does this method have a JavaImport attribute?
            /// </summary>
            public override bool HasJavaImportAttribute()
            {
                return method.HasJavaImportAttribute();
            }

            /// <summary>
            /// Try to get the names from the JavaImport attribute attached to this method.
            /// </summary>
            public override bool TryGetJavaImportNames(out string methodName, out string descriptor, out string className)
            {
                var attr = method.GetJavaImportAttribute();
                if (attr == null)
                {
                    methodName = null;
                    descriptor = null;
                    className = null;
                    return false;
                }
                attr.GetDexOrJavaImportNames(method, out methodName, out descriptor, out className);
                return true;
            }

            public void SetInheritedReturnType(TypeReference type)
            {
                OriginalReturnType = method.ReturnType;
                method.ReturnType = type;
            }

            private string GetScopeId()
            {
                // last resort: if we have a java/dex import attribute
                //              we will not get written to the mapfile
                //              make sure we are found by our descriptor.
                // these are not written in the map file.
                string methodName, descriptor, className;
                if (TryGetDexImportNames(out methodName, out descriptor, out className)
                 || TryGetJavaImportNames(out methodName, out descriptor, out className))
                {
                    return methodName + descriptor;
                }

                // this should work for normal method.
                var id = ((ILTypeDefinition)DeclaringType).GetMethodScopeId(method);
                if (id != null)
                    return id;

                // is it an internal generated method of which 
                // we know that it will never change, i.e. "$Clone" or "$CopyFrom"?
                if (Name.StartsWith("$"))
                    return Name;

                if (IsConstructor && method.Parameters.Count == 0 && !method.IsStatic)
                {
                    // a generated constructor. 
                    return "(new)";
                }

                // we are a generated method (most probably an explicit interface stub,
                // or a static class ctor.
                return "(none)";
            }
        }
    }
}
