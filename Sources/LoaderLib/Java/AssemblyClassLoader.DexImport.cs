using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;
using MethodAttributes = System.Reflection.MethodAttributes;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load java classes from JavaClass attributes included in assemblies.
    /// </summary>
    partial class AssemblyClassLoader
    {
        /// <summary>
        /// Data of a DexImportAttribute or JavaImportAttribute
        /// </summary>
        public sealed class DexImport
        {
            private readonly string className;
            private readonly CustomAttribute attr;
            private readonly List<TypeDefinition> types;
            private ClassFile resolved;
            private List<DexImportField> dexFields;
            private List<DexImportMethod> dexMethods;
            private readonly string scope;

            /// <summary>
            /// Default ctor
            /// </summary>
            internal DexImport(string className, TypeDefinition firstType, CustomAttribute attr, string scope)
            {
                this.className = className;
                this.attr = attr;
                this.scope = scope;
                types = new List<TypeDefinition> { firstType };
            }

            /// <summary>
            /// Gets the java classname.
            /// </summary>
            public string ClassName
            {
                get { return className; }
            }

            /// <summary>
            /// Points to deximport of declaring type (if nested)
            /// </summary>
            public DexImport DeclaringType { get; set; }

            /// <summary>
            /// Gets the first type from which to build a java class
            /// </summary>
            public TypeDefinition FirstType
            {
                get { return types[0]; }
            }

            /// <summary>
            /// Returns the dex/java import attribute
            /// </summary>
            public CustomAttribute ImportAttribute
            {
                get { return attr; }
            }

            /// <summary>
            /// Scope of the containing assembly
            /// </summary>
            public string Scope
            {
                get { return scope; }
            }

            /// <summary>
            /// Add a .NET type that holds members for my class.
            /// </summary>
            public void AddType(TypeDefinition type)
            {
                types.Add(type);
            }

            /// <summary>
            /// Resolve the payload into a class file.
            /// </summary>
            public ClassFile Resolve(IClassLoader loader, Action<ClassFile> classLoaded, List<DexImportMethod> methods)
            {
                if (resolved == null)
                {
                    // Load the class
                    List<DexImportField> tmpFields;
                    List<DexImportMethod> tmpMethods;
                    var tmpResolved = BuildClass(loader, out tmpFields, out tmpMethods);
                    // Keep the order of assignments for threading
                    dexFields = tmpFields;
                    dexMethods = tmpMethods;
                    resolved = tmpResolved;
                    if (classLoaded != null)
                    {
                        classLoaded(resolved);
                    }
                }
                if (methods != null)
                {
                    methods.AddRange(dexMethods);
                }
                return resolved;
            }

            /// <summary>
            /// Build a class from my data.
            /// </summary>
            private ClassFile BuildClass(IClassLoader loader, out List<DexImportField> fields, out List<DexImportMethod> methods)
            {
                // Build class
                var cf = new ClassFile(null, loader) { IsCreatedByLoader = true };
                cf.ClassName = className;
                cf.ClassAccessFlags = (ClassAccessFlags) GetAccessFlags();
                cf.SuperClass = GetSuperClassName();
                cf.SetSignature(GetSignature());

                // Add fields
                fields = types.SelectMany(x => x.Fields).Where(x => x.GetDexOrJavaImportAttribute() != null).Select(x => new DexImportField(x)).ToList();
                cf.Fields.AddRange(fields.Select(x => x.Resolve(cf)));

                // Add methods
                methods = types.SelectMany(x => x.Methods).Where(x => x.GetDexOrJavaImportAttribute() != null).Select(x => new DexImportMethod(x, x.GetDexOrJavaImportAttribute())).ToList();
                cf.Methods.AddRange(methods.Select(x => x.Resolve(cf)));

                return cf;
            }

            /// <summary>
            /// Gets the access flags to use for the class.
            /// </summary>
            private int GetAccessFlags()
            {
                foreach (var type in types)
                {
                    var attr = type.GetDexOrJavaImportAttribute(true);
                    var flags = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeAccessFlagsName).Select(x => (int) x.Argument.Value).FirstOrDefault();
                    if (flags != 0)
                        return flags;
                }
                return 0;
            }

            /// <summary>
            /// Gets the descriptor to use for the class.
            /// </summary>
            private string GetSignature()
            {
                foreach (var type in types)
                {
                    var attr = type.GetDexOrJavaImportAttribute(true);
                    var signature = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeSignature).Select(x => (string)x.Argument.Value).FirstOrDefault();
                    if (signature != null)
                        return signature;
                }
                return null;
            }

            /// <summary>
            /// Gets the classname of the super class.
            /// </summary>
            private ObjectTypeReference GetSuperClassName()
            {
                if (types.Any(x => x.IsInterface))
                    return new ObjectTypeReference("java/lang/Object", null);
                foreach (var type in types.Where(x => x.BaseType != null))
                {
                    var baseType = type.BaseType.Resolve();
                    if (baseType != null)
                    {
                        var attr = baseType.GetDexOrJavaImportAttribute();
                        if (attr != null)
                            return new ObjectTypeReference((string) attr.ConstructorArguments[0].Value, null);
                    }
                }
                return null;
            }

            /// <summary>
            /// Try to find a method by its name and descriptor.
            /// </summary>
            public bool TryGetMethod(IClassLoader classLoader, Action<ClassFile> classLoaded, string name, string signature, out DexImportMethod method)
            {
                var cf = Resolve(classLoader, classLoaded, null);
                signature = Descriptors.StripMethodReturnType(signature);
                foreach (var importMethod in dexMethods)
                {
                    var javaMethod = importMethod.Resolve(cf);
                    if ((javaMethod.Name == name) && (Descriptors.StripMethodReturnType(javaMethod.Descriptor) == signature))
                    {
                        method = importMethod;
                        return true;
                    }
                }
                method = null;
                return false;
            }

            /// <summary>
            /// Is the given java class name + method name + method descriptor a virtual method?
            /// </summary>
            public bool IsImportedVirtualMethod(IClassLoader classLoader, Action<ClassFile> classLoaded, string name, string descriptor, out MethodAttributes methodAttributes, out string netName, out string baseDescriptor, out string baseSignature)
            {
                var cf = Resolve(classLoader, classLoaded, null);
                descriptor = Descriptors.StripMethodReturnType(descriptor);
                foreach (var importMethod in dexMethods)
                {
                    var method = importMethod.Resolve(cf);
                    if ((method.Name == name) && (Descriptors.StripMethodReturnType(method.Descriptor) == descriptor))
                    {
                        methodAttributes = (MethodAttributes)importMethod.Method.Attributes;
                        netName = importMethod.Method.Name;
                        baseDescriptor = method.Descriptor;
                        baseSignature = (method.Signature != null) ? method.Signature.Original : null;
                        if (baseSignature == baseDescriptor) baseSignature = null;
                        return true;
                    }
                }
                methodAttributes = 0;
                netName = null;
                baseDescriptor = null;
                baseSignature = null;
                return false;
            }
        }
    }
}
