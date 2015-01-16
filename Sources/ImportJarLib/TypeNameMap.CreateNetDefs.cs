using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Cecil;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;
using Mono.Cecil;
using MethodAttributes = System.Reflection.MethodAttributes;
using TypeAttributes = System.Reflection.TypeAttributes;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Map type names from java to .NET
    /// </summary>
    partial class TypeNameMap 
    {
        private readonly List<NetTypeImplementationBuilder> implementationBuilders = new List<NetTypeImplementationBuilder>();

        /// <summary>
        /// Add the given assembly to the list of import assemblies.
        /// </summary>
        public void ImportAssembly(AssemblyDefinition assembly, Action<ClassFile> classLoaded, TargetFramework target)
        {
            assemblyClassLoader.LoadAssembly(assembly, x => InitializeMapping(x, classLoaded, target));
        }

        /// <summary>
        /// Build remainder parts of NetTypeDefinition's
        /// </summary>
        public void ImportAssembliesCompleted(TargetFramework target)
        {
            foreach (var builder in implementationBuilders)
            {
                builder.Implement(target);
            }
        }

        /// <summary>
        /// Create a mapping for each of the DexImport classes.
        /// </summary>
        private void InitializeMapping(AssemblyClassLoader.AssemblyClasses assemblyClasses, Action<ClassFile> classLoaded, TargetFramework target)
        {
            foreach (var dexImport in assemblyClasses.DexOrJavaImportTypes.OrderBy(x => x.ClassName))
            {
                Add(dexImport.ClassName, CreateNetTypeDef(dexImport, classLoaded, ResolveDexImport, target));
            }
        }

        /// <summary>
        /// Get a dex import from the given type.
        /// Throw an exception if not found.
        /// </summary>
        private AssemblyClassLoader.DexImport ResolveDexImport(TypeDefinition type)
        {
            AssemblyClassLoader.DexImport dexImport;
            if (assemblyClassLoader.TryGetDexImport(type, out dexImport))
                return dexImport;
            throw new ImportException(string.Format("Cannot find dex info for type {0}", type.FullName));
        }

        /// <summary>
        /// Create a NetTypeDefinition wrapper for the given dex import.
        /// </summary>
        private NetTypeDefinition CreateNetTypeDef(AssemblyClassLoader.DexImport dexImport, Action<ClassFile> classLoaded,
            Func<TypeDefinition, AssemblyClassLoader.DexImport> type2DexImport, TargetFramework target)
        {
            // Convert dex import to class file
            var dexMethods = new List<AssemblyClassLoader.DexImportMethod>();
            var classFile = dexImport.Resolve(assemblyClassLoader, classLoaded, dexMethods);

            // Build type-definition
            var typeDef = new NetTypeDefinition(classFile, target, dexImport.Scope)
            {
                Namespace = dexImport.FirstType.Namespace,
                Name = StripGenerics(dexImport.FirstType.Name),
                Attributes = (TypeAttributes) dexImport.FirstType.Attributes,
                ImportedFromMapping = true
            };

            // Build custom attribute
            typeDef.CustomAttributes.Add(BuildCustomAttribute(dexImport.ImportAttribute));

            // Add generic parameters
            if (dexImport.FirstType.HasGenericParameters)
            {
                foreach (var gp in dexImport.FirstType.GenericParameters)
                {
                    typeDef.GenericParameters.Add(new NetGenericParameter(gp.Name, gp.Name, typeDef));
                }
            }

            // Record builder to complete later
            implementationBuilders.Add(new NetTypeImplementationBuilder(this, classFile, typeDef, dexImport, dexMethods, type2DexImport));

            return typeDef;
        }

        /// <summary>
        /// String the generic count of the given name
        /// </summary>
        private static string StripGenerics(string name)
        {
            var index = name.IndexOf('`');
            return (index > 0) ? name.Substring(0, index) : name;
        }

        /// <summary>
        /// Convert the given IL custom attribute to a NetCustomAttribute.
        /// </summary>
        private static NetCustomAttribute BuildCustomAttribute(CustomAttribute attr)
        {
            var ca = new NetCustomAttribute(null, attr.ConstructorArguments.Select(x => x.Value).ToArray());
            foreach (var pv in attr.Properties)
            {
                ca.Properties.Add(pv.Name, pv.Argument.Value);
            }
            return ca;
        }

        /// <summary>
        /// Helper used to implement the majority of a NetTypeDefinition.
        /// </summary>
        private class NetTypeImplementationBuilder
        {
            private readonly List<AssemblyClassLoader.DexImportMethod> dexMethods;
            private readonly TypeNameMap typeNameMap;
            private readonly ClassFile classFile;
            private readonly NetTypeDefinition typeDef;
            private readonly AssemblyClassLoader.DexImport dexImport;
            private readonly Func<TypeDefinition, AssemblyClassLoader.DexImport> type2DexImport;

            /// <summary>
            /// Default ctor
            /// </summary>
            public NetTypeImplementationBuilder(TypeNameMap typeNameMap, ClassFile classFile, NetTypeDefinition typeDef, AssemblyClassLoader.DexImport dexImport, 
                List<AssemblyClassLoader.DexImportMethod> dexMethods, 
                Func<TypeDefinition, AssemblyClassLoader.DexImport> type2DexImport)
            {
                this.typeNameMap = typeNameMap;
                this.classFile = classFile;
                this.typeDef = typeDef;
                this.dexMethods = dexMethods;
                this.type2DexImport = type2DexImport;
                this.dexImport = dexImport;
            }

            /// <summary>
            /// Build all remaining structures.
            /// </summary>
            public void Implement(TargetFramework target)
            {
                // Set declaring type (if any)
                if (dexImport.DeclaringType != null)
                {
                    typeDef.DeclaringType = typeNameMap.GetByJavaClassName(dexImport.DeclaringType.ClassName);
                }

                // Set base type (if any)
                var baseTypeRef = dexImport.FirstType.BaseType;
                if (baseTypeRef != null)
                {
                    if (baseTypeRef.FullName == "System.ValueType")
                    {
                        typeDef.IsEnum = true;
                    }
                    else
                    {
                        typeDef.BaseType = Resolve(baseTypeRef, dexImport.FirstType, typeDef);
                    }
                }

                // Add methods
                foreach (var dexMethod in dexMethods)
                {
                    if (dexMethod.IgnoreFromJava)
                        continue;

                    var ilMethod = dexMethod.Method;
                    var javaMethod = dexMethod.Resolve(classFile);

                    // Create method
                    var method = new NetMethodDefinition(ilMethod.Name, javaMethod, typeDef, target, false, "DexImport");
                    method.Attributes = (MethodAttributes)ilMethod.Attributes;
                    method.AccessFlags = (int)javaMethod.AccessFlags;

                    // Set generic parameters
                    foreach (var gp in dexMethod.Method.GenericParameters)
                    {
                        method.GenericParameters.Add(new NetGenericParameter(gp.Name, gp.Name, method));
                    }

                    // Set return type
                    method.ReturnType = Resolve(ilMethod.ReturnType, ilMethod, method);

                    // Set parameters
                    foreach (var p in ilMethod.Parameters)
                    {
                        var np = new NetParameterDefinition(p.Name, Resolve(p.ParameterType, ilMethod, method), false /*todo*/);
                        method.Parameters.Add(np);
                    }

                    // Add custom attribute
                    method.CustomAttributes.Add(BuildCustomAttribute(dexMethod.ImportAttribute));

                    // Add to typeDef
                    typeDef.Methods.Add(method);
                }
                
            }

            /// <summary>
            /// Resolve the given type to a NetTypeReference.
            /// </summary>
            private NetTypeReference Resolve(TypeReference type, IGenericContext context, INetGenericParameterProvider provider)
            {
                return type.Accept(ResolveVisitor.Instance, new ResolveData(this, context, provider));
            }

            /// <summary>
            /// Container for resolve data
            /// </summary>
            private sealed class ResolveData
            {
                public readonly NetTypeImplementationBuilder Builder;
                public readonly IGenericContext GenericContext;
                public readonly INetGenericParameterProvider GenericParameterProvider;

                public ResolveData(NetTypeImplementationBuilder builder, IGenericContext genericContext, INetGenericParameterProvider genericParameterProvider)
                {
                    Builder = builder;
                    GenericContext = genericContext;
                    GenericParameterProvider = genericParameterProvider;
                }
            }

            /// <summary>
            /// Helper class used to resolve type references to NetTypeReference's.
            /// </summary>
            private sealed class ResolveVisitor : ITypeVisitor<NetTypeReference, ResolveData>
            {
                internal static readonly ResolveVisitor Instance = new ResolveVisitor();

                public NetTypeReference Visit(TypeDefinition type, ResolveData data)
                {
                    var dexImport = data.Builder.type2DexImport(type);
                    return data.Builder.typeNameMap.GetByJavaClassName(dexImport.ClassName);
                }

                public NetTypeReference Visit(TypeReference type, ResolveData data)
                {
                    var typeDef = type.Resolve();
                    if (typeDef == null)
                        throw new ImportException(string.Format("Cannot resolve type {0}", type.FullName));
                    return Visit(typeDef, data);
                }

                public NetTypeReference Visit(GenericParameter type, ResolveData data)
                {
                    if (type.Owner.GenericParameterType == GenericParameterType.Type)
                    {
                        var owner = (NetTypeDefinition)((TypeReference) type.Owner).Accept(this, data);
                        return owner.GenericParameters[type.Position];
                    }
                    if (type.Owner == data.GenericContext)
                    {
                        var owner = data.GenericParameterProvider;
                        return owner.GenericParameters[type.Position];
                    }
                    throw new NotImplementedException("GenericParameter of methods is not supported");
                }

                public NetTypeReference Visit(GenericInstanceType type, ResolveData data)
                {
                    if (type.ElementType.FullName == "System.Nullable`1")
                    {
                        var arg = type.GenericArguments.Single();
                        return new NetNullableType(arg.Accept(this, data));
                    }
                    else
                    {
                        var declaringType = (type.DeclaringType != null) ? type.DeclaringType.Accept(this, data) : null;
                        var elementType = type.ElementType.Accept(this, data);
                        var git = new NetGenericInstanceType((NetTypeDefinition) elementType, declaringType);
                        foreach (var arg in type.GenericArguments)
                        {
                            git.AddGenericArgument(arg.Accept(this, data), null);
                        }
                        return git;
                    }
                }

                public NetTypeReference Visit(ArrayType type, ResolveData data)
                {
                    var elementType = type.ElementType.Accept(this, data);
                    return new NetArrayType(elementType);
                }

                public NetTypeReference Visit(ByReferenceType type, ResolveData data)
                {
                    throw new NotImplementedException("ByReferenceType to NetTypeReference");
                }

                public NetTypeReference Visit(FunctionPointerType type, ResolveData data)
                {
                    throw new NotImplementedException("FunctionPointerType to NetTypeReference");
                }

                public NetTypeReference Visit(OptionalModifierType type, ResolveData data)
                {
                    throw new NotImplementedException("OptionalModifierType to NetTypeReference");
                }

                public NetTypeReference Visit(RequiredModifierType type, ResolveData data)
                {
                    throw new NotImplementedException("RequiredModifierType to NetTypeReference");
                }

                public NetTypeReference Visit(PinnedType type, ResolveData data)
                {
                    throw new NotImplementedException("PinnedType to NetTypeReference");
                }

                public NetTypeReference Visit(PointerType type, ResolveData data)
                {
                    throw new NotImplementedException("PointerType to NetTypeReference");
                }

                public NetTypeReference Visit(SentinelType type, ResolveData data)
                {
                    throw new NotImplementedException("SentinelType to NetTypeReference");
                }
            }
        }
    }
}
