using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;
using Mono.Cecil;
using MethodAttributes = System.Reflection.MethodAttributes;
using TypeAttributes = System.Reflection.TypeAttributes;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Map type names from java to .NET
    /// </summary>
    public sealed partial class TypeNameMap : IClassLoader
    {
        private readonly ITypeMapResolver resolver;
        private readonly Dictionary<string, NetTypeDefinition> map = new Dictionary<string, NetTypeDefinition>();
        private readonly AssemblyClassLoader assemblyClassLoader;
        //private readonly List<JarImportMapping> importMappings = new List<JarImportMapping>();
        private NetTypeDefinition objectDef;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TypeNameMap(ITypeMapResolver resolver, AssemblyClassLoader assemblyClassLoader)
        {
            this.resolver = resolver;
            this.assemblyClassLoader = assemblyClassLoader;
        }

        /// <summary>
        /// Add the given mapping.
        /// </summary>
        /// <remarks>
        /// External jars might include some standard classes which we also
        /// have included in our libraries. Since the classnames can be taken
        /// as unique (for example org.xmlpull.v1.XmlSerializer), we should
        /// be able to safely overwrite the first addition of the class.
        /// The MS .net compiler will use the one in the external jar, not the
        /// one in the dot42 library. So that also suggests we need to ignore
        /// the first addition (Peter Laudy).
        /// </remarks>
        public void Add(string javaClassName, NetTypeDefinition clrType)
        {
            map[javaClassName] = clrType;
        }

        /// <summary>
        /// Gets a mapping
        /// </summary>
        public NetTypeReference GetType(ObjectTypeReference javaTypeRef, TargetFramework target, IBuilderGenericContext gcontext)
        {
            NetTypeReference result;
            if (TryGetType(javaTypeRef, target, gcontext, out result))
                return result;

            //var names = map.Keys.OrderBy(x => x).ToArray();
            throw new ArgumentException(string.Format("{0} not found", javaTypeRef));
        }


        /// <summary>
        /// Gets a mapping
        /// </summary>
        public bool TryGetType(ObjectTypeReference javaTypeRef, TargetFramework target, IBuilderGenericContext gcontext, out NetTypeReference result)
        {
            NetTypeDefinition typeDef;
            if (TryGetByJavaClassName(javaTypeRef.ClassName, out typeDef))
            {
                // Custom
                /*if (typeDef.FullName.StartsWith("System.Type"))
                {
                }*/

                // Create result
                if (!javaTypeRef.Arguments.Any() && (typeDef.GenericParameters.Count == 0))
                {
                    // Normal non-generic type
                    result = typeDef;
                    return true;
                }

                if (typeDef.IgnoreGenericArguments)
                {
                    // Force use as normal type
                    result = typeDef;
                    return true;
                }

                // Generic type is used as non-generic?
                NetTypeReference declaringType = null;
                if (javaTypeRef.Prefix != null)
                {
                    TryGetType(javaTypeRef.Prefix, target, gcontext, out declaringType);
                }
                var git = new NetGenericInstanceType(typeDef, declaringType);
                if (!javaTypeRef.Arguments.Any() && (typeDef.GenericParameters.Count > 0))
                {
                    // Add "object" arguments
                    foreach (var tp in typeDef.GenericParameters)
                    {
                        git.AddGenericArgument(Object, this);
                    }
                    result = git;
                    return true;
                }

                if (javaTypeRef.Arguments.Count() != typeDef.GenericParameters.Count)
                {
                    if ((resolver == null) || (typeDef.GenericParameters.Count != 0) ||
                        (!resolver.AcceptLackOfGenericParameters))
                    {
                        throw new ArgumentException(string.Format("Mismatch between generic parameter count and generic argument count in {0}", javaTypeRef));
                    }
                    if ((resolver != null) && (resolver.AcceptLackOfGenericParameters) &&
                        (typeDef.GenericParameters.Count == 0))
                    {
                        result = typeDef;
                        return true;
                    }
                }

                // Type with generic arguments
                foreach (var typeArg in javaTypeRef.Arguments)
                {
                    NetTypeReference arg;
                    if (typeArg.IsAny)
                    {
                        arg = Object;
                    }
                    else
                    {
                        if (!typeArg.Signature.TryResolve(target, gcontext, false, out arg))
                        {
                            arg = Object;
                            //result = null;
                            //return false;
                        }
                    }
                    git.AddGenericArgument(arg, this);
                }
                result = git;
                return true;
            }
#if DEBUG
            var names = map.Keys.OrderBy(x => x).ToArray();
#endif
            //throw new ArgumentException(string.Format("{0} not found", javaTypeRef));
            result = null;
            return false;
        }

        /// <summary>
        /// Gets a mapping.
        /// Returns true if found, false otherwise.
        /// </summary>
        public NetTypeDefinition TryGetByJavaClassName(string javaClassName)
        {
            NetTypeDefinition result;
            return map.TryGetValue(javaClassName, out result) ? result : null;
        }

        /// <summary>
        /// Gets a mapping.
        /// Throws an error if not found.
        /// </summary>
        public bool TryGetByJavaClassName(string javaClassName, out NetTypeDefinition result)
        {
            if (map.TryGetValue(javaClassName, out result))
                return true;
            if (resolver != null)
            {
                // Try to resolve
                resolver.TryResolve(javaClassName, this);

                // Try the get again
                if (map.TryGetValue(javaClassName, out result))
                    return true;
            }
#if DEBUG
            var names = map.Keys.OrderBy(x => x).ToArray();
#endif
            return false;
        }

        /// <summary>
        /// Gets a mapping.
        /// Throws an error if not found.
        /// </summary>
        public NetTypeDefinition GetByJavaClassName(string javaClassName)
        {
            NetTypeDefinition result;
            if (TryGetByJavaClassName(javaClassName, out result))
                return result;
            throw new ClassNotFoundException(javaClassName);
        }

        /// <summary>
        /// Are there any import mappings?
        /// </summary>
        public bool HasImportMappings { get { return !assemblyClassLoader.IsEmpty; } }

        /// <summary>
        /// Gets a mapping.
        /// Throws an error if not found.
        /// </summary>
        public NetTypeDefinition GetByType(Type netType)
        {
            var result = map.Values.FirstOrDefault(x => x.FullName == netType.FullName);
            if (result != null) return result;
            if (resolver != null)
            {
                resolver.TryResolve(netType, this);
            }
            return map.Values.First(x => x.FullName == netType.FullName);
        }

        /// <summary>
        /// Gets System.Object
        /// </summary>
        public NetTypeDefinition Object
        {
            get { return objectDef ?? (objectDef = GetByType(typeof (object))); }
        }

        /// <summary>
        /// Try to load a class from import mappings.
        /// </summary>
        bool IClassLoader.TryLoadClass(string className, out ClassFile result)
        {
            return assemblyClassLoader.TryLoadClass(className, out result);
        }

        /// <summary>
        /// Gets all package names found in this loader.
        /// </summary>
        IEnumerable<string> IClassLoader.Packages
        {
            get { return assemblyClassLoader.Packages; }
        }
    }
}
