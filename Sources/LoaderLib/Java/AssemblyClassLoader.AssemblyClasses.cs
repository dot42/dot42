using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;

namespace Dot42.LoaderLib.Java
{
    /// <summary>
    /// Load java classes from JavaClass attributes included in assemblies.
    /// </summary>
    partial class AssemblyClassLoader 
    {
        /// <summary>
        /// Holds the JavaClass attributes of a specific assembly and the DexImport't classes.
        /// </summary>
        public sealed class AssemblyClasses
        {
            private readonly AssemblyDefinition assembly;
            private readonly Dictionary<string, JavaCode> javaCodes = new Dictionary<string, JavaCode>();
            private readonly Dictionary<string, JavaClass> javaClasses = new Dictionary<string, JavaClass>();
            private readonly Dictionary<string, DexImport> className2DexImportMap = new Dictionary<string, DexImport>();
            private readonly Dictionary<TypeDefinition, DexImport> type2DexImportMap = new Dictionary<TypeDefinition, DexImport>();

            /// <summary>
            /// Default ctor
            /// </summary>
            internal AssemblyClasses(AssemblyDefinition assembly, Action<ClassSource> jarLoaded=null)
            {
                this.assembly = assembly;
                foreach (var attr in assembly.GetJavaCodeAttributes())
                {
                    var resourceName = (string)attr.ConstructorArguments[0].Value;
                    var fileName = attr.ConstructorArguments.Count > 1 ? (string)attr.ConstructorArguments[1].Value : null;

                    JavaCode javaCode;
                    if (!javaCodes.TryGetValue(resourceName, out javaCode))
                    {
                        var resource = assembly.MainModule.Resources.FirstOrDefault(x => x.Name == resourceName) as EmbeddedResource;
                        if (resource == null)
                            throw new LoaderException("Cannot find resource " + resourceName);
                        javaCode = new JavaCode(resource, fileName);
                        javaCodes[resourceName] = javaCode;

                        if (jarLoaded != null)
                            jarLoaded(javaCode.ClassSource);

                        foreach (var classFileName in javaCode.Resolve(null).ClassFileNames)
                        {
                            var className = classFileName;
                            if (className.EndsWith(".class", StringComparison.OrdinalIgnoreCase))
                            {
                                className = className.Substring(0, className.Length - ".class".Length);
                            }
                            var jClass = new JavaClass(className, javaCode);
                            javaClasses[className] = jClass;
                        }
                    }
                }
                var scope = AssemblyResolver.IsFrameworkAssembly(assembly) ? AttributeConstants.Dot42Scope : assembly.FullName;
                var ignoreFromJavaTypes = new List<TypeDefinition>();
                foreach (var type in assembly.MainModule.Types)
                {
                    CollectDexImportClasses(type, className2DexImportMap, type2DexImportMap, null, scope, ignoreFromJavaTypes);
                }
                foreach (var type in ignoreFromJavaTypes)
                {
                    var attr = type.GetDexOrJavaImportAttribute();
                    var className = (string)attr.ConstructorArguments[0].Value;
                    DexImport dexImport;
                    if (className2DexImportMap.TryGetValue(className, out dexImport))
                    {
                        dexImport.AddType(type);
                        type2DexImportMap[type] = dexImport;
                    }
                }
            }

            /// <summary>
            /// Gets a java class by it's classname.
            /// </summary>
            /// <returns>False if not found.</returns>
            internal bool TryGetJavaClass(string className, out JavaClass jClass)
            {
                return javaClasses.TryGetValue(className, out jClass);
            }

            /// <summary>
            /// Gets all class names found in this loader.
            /// </summary>
            internal ICollection<string> ClassNames
            {
                get { return javaClasses.Keys; }
            }

            /// <summary>
            /// Gets all package names found in this loader.
            /// </summary>
            internal IEnumerable<string> Packages
            {
                get { return javaClasses.Keys.Select(ClassName.GetPackage).Distinct(); }
            }

            /// <summary>
            /// Try to get dex import data for the given class name?
            /// </summary>
            /// <returns>False if not found.</returns>
            public bool TryGetDexImport(string className, out DexImport dexImport)
            {
                return className2DexImportMap.TryGetValue(className, out dexImport);
            }

            /// <summary>
            /// Try to get dex import data for the given type definition?
            /// </summary>
            /// <returns>False if not found.</returns>
            public bool TryGetDexImport(TypeDefinition type, out DexImport dexImport)
            {
                return type2DexImportMap.TryGetValue(type, out dexImport);
            }

            /// <summary>
            /// Gets the assembly the classes were loaded from.
            /// </summary>
            public AssemblyDefinition Assembly { get { return assembly; } }

            /// <summary>
            /// Gets all types with DexImport attributes.
            /// </summary>
            public IEnumerable<DexImport> DexOrJavaImportTypes { get { return className2DexImportMap.Values; } }

            /// <summary>
            /// Collect the class names of all dex import types.
            /// </summary>
            private static void CollectDexImportClasses(TypeDefinition type, Dictionary<string, DexImport> className2DexImports, 
                Dictionary<TypeDefinition, DexImport> type2DexImports, DexImport declaringType, string scope, List<TypeDefinition> ignoreFromJavaTypes)
            {
                var attr = type.GetDexOrJavaImportAttribute();
                if (attr == null)
                    return;
                if (attr.HasProperties)
                {
                    var ignoreFromJava = attr.Properties.Where(x => x.Name == AttributeConstants.DexImportAttributeIgnoreFromJavaName).Select(x => x.Argument.Value).FirstOrDefault();
                    if ((ignoreFromJava is bool) && ((bool) ignoreFromJava))
                    {
                        ignoreFromJavaTypes.Add(type);
                        return;
                    }
                }
                var className = (string)attr.ConstructorArguments[0].Value;
                DexImport existing;
                if (className2DexImports.TryGetValue(className, out existing))
                {
                    existing.AddType(type);
                }
                else
                {
                    className2DexImports[className] = existing = new DexImport(className, type, attr, scope) { DeclaringType = declaringType };
                }
                type2DexImports[type] = existing;
                if (type.HasNestedTypes)
                {
                    foreach (var nested in type.NestedTypes)
                    {
                        CollectDexImportClasses(nested, className2DexImports, type2DexImports, existing, scope, ignoreFromJavaTypes);
                    }
                }
            }
        }
    }
}
