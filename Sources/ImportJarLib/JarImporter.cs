using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.DotNet;
using Dot42.Utility;

namespace Dot42.ImportJarLib
{
    public class JarImporter : ICodeGeneratorContext, ITypeMapResolver
    {
        private readonly string jarFilePath;
        private readonly string libraryName;
        private readonly bool importAsStubs;
        private readonly string sourceOutputPath;
        private readonly AssemblyResolver assemblyResolver;
        private readonly TargetFramework target;

        private static readonly string[] AutoExcludedPackages = new[] {
            /* Google Play Services */
            "com.google.android.gms.internal" 
        };

        /// <summary>
        /// Default ctor
        /// </summary>
        public JarImporter(string jarFilePath, string libraryName, bool importAsStubs, bool importPublicOnly, string sourceOutputPath, AssemblyResolver assemblyResolver, IEnumerable<string> excludedPackages, bool useAutoExcludedPackages)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            this.jarFilePath = jarFilePath;
            this.libraryName = libraryName;
            this.importAsStubs = importAsStubs;
            this.sourceOutputPath = sourceOutputPath;
            this.assemblyResolver = assemblyResolver;
            if (useAutoExcludedPackages)
            {
                excludedPackages = AutoExcludedPackages;
            }
            target = new TargetFramework(this, assemblyResolver.ClassLoader, new DocModel(), null, importAsStubs, importPublicOnly, excludedPackages);
        }

        /// <summary>
        /// Load a type name map.
        /// </summary>
        public void ImportAssembly(string assemblyName)
        {
            var assembly = assemblyResolver.Resolve(assemblyName);
            target.ImportAssembly(assembly, null);
        }

        /// <summary>
        /// All assemblies have been imported.
        /// </summary>
        public void ImportAssembliesCompleted()
        {
            target.ImportAssembliesCompleted();
        }

        /// <summary>
        /// Perform the import from jar to cs source.
        /// </summary>
        public void Import()
        {
            using (var jf = new JarFile(File.OpenRead(jarFilePath), jarFilePath, target.TypeNameMap))
            {
                // Create output folder
                var folder = Path.GetFullPath(sourceOutputPath);
                Directory.CreateDirectory(folder);

                // Create mscorlib
                CreateAssembly(jf, folder);
            }
        }

        /// <summary>
        /// Create wrapper for given jar file
        /// </summary>
        private void CreateAssembly(JarFile jf, string folder)
        {
            // Create java type wrappers
            var module = new NetModule(jf.Scope);

            var classTypeBuilders = jf.ClassNames.SelectMany(n => StandardTypeBuilder.Create(jf.LoadClass(n), target));

            var typeBuilders = classTypeBuilders.OrderBy(x => x.Priority).ToList();
            typeBuilders.ForEach(x => x.CreateType(null, module, target));

            // Implement and finalize types
            Implement(typeBuilders, target);

            var assemblyAttributes = new List<NetCustomAttribute>();
            if (!importAsStubs)
            {
                // Import code
                var attrType = new NetTypeDefinition(ClassFile.Empty, target, AttributeConstants.Dot42Scope)
                {
                    Name = AttributeConstants.JavaCodeAttributeName,
                    Namespace = AttributeConstants.Dot42AttributeNamespace
                };
                var hash = JarReferenceHash.ComputeJarReferenceHash(jarFilePath);
                var attr = new NetCustomAttribute(attrType, hash);
                assemblyAttributes.Add(attr);
            }

            // Save
            CodeGenerator.Generate(folder, module.Types, assemblyAttributes, target, this, target);
        }

        /// <summary>
        /// Implement and finalize types
        /// </summary>
        public static void Implement(List<TypeBuilder> typeBuilders, TargetFramework target)
        {
            typeBuilders.ForEach(x => x.Implement(target));
            typeBuilders.ForEach(x => x.Finalize(target, FinalizeStates.AddRemoveMembers));
            typeBuilders.ForEach(x => x.Finalize(target, FinalizeStates.FixTypes));
            var methodRenamer = new MethodRenamer(target);
            typeBuilders.ForEach(x => x.FinalizeNames(target, methodRenamer));
            typeBuilders.ForEach(x => x.FinalizeProperties(target));
            typeBuilders.ForEach(x => x.FinalizeVisibility(target));
        }

        /// <summary>
        /// Add code to the header of a source file.
        /// </summary>
        void ICodeGeneratorContext.CreateSourceFileHeader(TextWriter writer)
        {
            writer.WriteLine("#pragma warning disable 108"); // disable warning hidden inherited members
            writer.WriteLine("#pragma warning disable 649"); // disable warning field is never assigned to
            writer.WriteLine("#pragma warning disable 659"); // disable warning overrides Equals but not GetHashCode
            writer.WriteLine("#pragma warning disable 693"); // disable warning names of nested type parameters
            writer.WriteLine("#pragma warning disable 824"); // disable warning about external constructors
            if (importAsStubs)
            {
                writer.WriteLine("[assembly: Dot42.Manifest.UsesLibrary(\"{0}\")]", libraryName);
            }
        }

        /// <summary>
        /// If true, all methods are generated as extern.
        /// </summary>
        bool ICodeGeneratorContext.GenerateExternalMethods { get { return true; } }

        /// <summary>
        /// If true, will output debug releated comments
        /// </summary>
        bool ICodeGeneratorContext.GenerateDebugComments
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets all roots that can come our of <see cref="GetNamespaceRoot"/>
        /// </summary>
        IEnumerable<string> ICodeGeneratorContext.PossibleNamespaceRoots { get { yield return libraryName; } }

        /// <summary>
        /// Gets the root part of the namespace.
        /// The result of this method is used for the name of the source file in which the type is placed.
        /// </summary>
        string ICodeGeneratorContext.GetNamespaceRoot(NetTypeDefinition type)
        {
            return libraryName;
        }

        /// <summary>
        /// Try to resolve the given java class name.
        /// </summary>
        void ITypeMapResolver.TryResolve(string javaClassName, TypeNameMap typeNameMap)
        {
            // Do nothing
        }

        /// <summary>
        /// Try to resolve the given NET name.
        /// </summary>
        void ITypeMapResolver.TryResolve(Type netType, TypeNameMap typeNameMap)
        {
            // Auto create a typedef.
            var typeDef = new NetTypeDefinition(ClassFile.Empty, target, AttributeConstants.Dot42Scope) { Namespace = netType.Namespace, Name = netType.Name};
            typeNameMap.Add(netType.FullName, typeDef);
        }

        /// <summary>
        /// If true, a lack of generic parameters is accepted.
        /// </summary>
        bool ITypeMapResolver.AcceptLackOfGenericParameters { get { return true; } }
    }
}
