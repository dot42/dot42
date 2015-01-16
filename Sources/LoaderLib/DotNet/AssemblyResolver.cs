using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.LoaderLib.Java;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.LoaderLib.DotNet
{
    /// <summary>
    /// Resolve assemblies.
    /// </summary>
    public class AssemblyResolver : IAssemblyResolver
    {
        private readonly AssemblyClassLoader classLoader;
        private readonly List<string> referenceFolders;
        private readonly Dictionary<string, AssemblyDefinition> references = new Dictionary<string, AssemblyDefinition>();
        private readonly Action<AssemblyDefinition> assemblyLoaded;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AssemblyResolver(IEnumerable<string> referenceFolders, AssemblyClassLoader classLoader, Action<AssemblyDefinition> assemblyLoaded)
        {
            this.classLoader = classLoader;
            this.assemblyLoaded = assemblyLoaded;
            this.referenceFolders = referenceFolders.Select(ToFolder).Distinct().ToList();
        }

        /// <summary>
        /// Is the given path is a file, return it's folder, otherwise return the given path.
        /// </summary>
        private static string ToFolder(string path)
        {
            if (File.Exists(path))
                return Path.GetDirectoryName(path);
            return path;
        }

        /// <summary>
        /// Gets the class loader attached to this resolver.
        /// </summary>
        public AssemblyClassLoader ClassLoader
        {
            get { return classLoader; }
        }

        /// <summary>
        /// Load an assembly for compiler from the given path and record it in the references.
        /// </summary>
        public AssemblyDefinition Load(string path, ReaderParameters parameter)
        {
            var fullPath = ResolvePath(path);
            if (fullPath == null)
                throw new FileNotFoundException(path);
            var asm = AssemblyDefinition.ReadAssembly(fullPath, parameter);
            AssemblyDefinition existing;
            var key = asm.Name.Name;
            if (references.TryGetValue(key, out existing))
                return existing;
            references.Add(key, asm);
            return asm;
        }

        /// <summary>
        /// Resolve an assembly by name.
        /// </summary>
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, new ReaderParameters(ReadingMode.Deferred) {
                AssemblyResolver = this,
                SymbolReaderProvider = new SafeSymbolReaderProvider(),
                ReadSymbols = true,
            });
        }

        /// <summary>
        /// Resolve an assembly by name with given parameters.
        /// </summary>
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            AssemblyDefinition result;
            if (references.TryGetValue(name.Name, out result))
                return result;

            var path = ResolvePath(name.Name);
            if (path == null)
            {
                DLog.Error(DContext.CompilerAssemblyResolver, "Failed to resolve assembly {0} in {1}", name, string.Join(";  ", referenceFolders));
                throw new AssemblyResolutionException(name);
            }

            try
            {
                Console.WriteLine(string.Format("Loading {0}", name.Name));
                var reference = AssemblyDefinition.ReadAssembly(path, parameters);
                references[name.Name] = reference;
                VerifyFrameworkAssembly(reference, path);
                if (assemblyLoaded != null)
                {
                    assemblyLoaded(reference);
                }
                if (classLoader != null)
                {
                    classLoader.LoadAssembly(reference);
                }
                return reference;
            }
            catch (Exception ex)
            {
                // Unload the reference
                references.Remove(name.Name);
#if DEBUG
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
#endif
                // Log the error
                DLog.Error(DContext.CompilerAssemblyResolver, "Failed to load assembly {0}", ex, name);

                // Pass the error on
                throw new AssemblyResolutionException(name);
            }
        }

        /// <summary>
        /// Resolve the given name into a path.
        /// </summary>
        private string ResolvePath(string path)
        {
            if (File.Exists(path))
                return path;
            var name = Path.GetFileName(path);
            var referencePaths = referenceFolders.Select(x => Path.Combine(x, name + ".dll"));
            referencePaths = referencePaths.Concat(referenceFolders.Select(x => Path.Combine(x, name)));
            path = referencePaths.FirstOrDefault(x => string.Equals(Path.GetFileNameWithoutExtension(x), name, StringComparison.OrdinalIgnoreCase) && File.Exists(x));
            return path;
        }

        /// <summary>
        /// Resolve an assembly by name.
        /// </summary>
        public AssemblyDefinition Resolve(string fullName)
        {
            var name = AssemblyNameReference.Parse(fullName);
            return Resolve(name);
        }

        /// <summary>
        /// Resolve an assembly by name with given parameters.
        /// </summary>
        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            var name = AssemblyNameReference.Parse(fullName);
            return Resolve(name, parameters);
        }

        /// <summary>
        /// Make sure a valid framework assembly is used (if it is a framework assembly).
        /// </summary>
        private static void VerifyFrameworkAssembly(AssemblyDefinition assembly, string path)
        {
            // Check if the assembly is a framework assembly.
            if (!IsFrameworkAssembly(assembly))
                return;

            // Check informational version
            var attr = assembly.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == AttributeConstants.AssemblyInformationalVersionAttributeFullName);
            if (attr == null)
            {
                throw new LoaderException(string.Format("Assembly {0} is not a valid framework assembly. [missing informational version]", assembly.Name));
            }
            var expectedPostfix = string.Format(", Dot42 {0}", typeof (AssemblyResolver).Assembly.GetName().Version);
            var infVersion = (attr.ConstructorArguments.Count != 0) ? attr.ConstructorArguments[0].Value as string : null;
            if ((infVersion == null) || !infVersion.EndsWith(expectedPostfix))
            {
                var msg = string.Format("Assembly {0} is not a valid framework assembly. [invalid informational version]", assembly.Name);
#if DEBUG
                Console.WriteLine(msg);
#else
                throw new LoaderException(string.Format(msg));                
#endif
            }

            // Check public key token
            var token = GetPublicKeyToken(assembly.Name);
            if (token != AssemblyConstants.SdkPublicKeyToken)
            {
                // throw new LoaderException(string.Format("Assembly {0} is not a valid framework assembly. [invalid token]", assembly.Name));                                
            }

            if (!SnToolResolver.VerifyAssembly(path))
            {
                // throw new LoaderException(string.Format("Assembly {0} is not a valid framework assembly. Strong name verification failed.", assembly.Name));
            }
        }

        /// <summary>
        /// Is the given assembly a Dot42 framework assembly?
        /// </summary>
        internal static bool IsFrameworkAssembly(AssemblyDefinition assembly)
        {
            // Dot42.*
            if (string.Equals(assembly.Name.Name, AssemblyConstants.SdkAssemblyName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Has FrameworkLibraryAttribute?
            if (assembly.HasCustomAttributes)
            {
                var fullName = AttributeConstants.Dot42AttributeNamespace + '.' +
                               AttributeConstants.FrameworkLibraryAttributeName;
                if (assembly.CustomAttributes.Any(x => x.AttributeType.FullName == fullName))
                    return true;
            }

            // Has Dot42 strong name key
            var token = GetPublicKeyToken(assembly.Name);
            if (token == AssemblyConstants.SdkPublicKeyToken)
                return true;            

            // Has System.Object type
            if (assembly.MainModule.GetType("System", "Object") != null)
                return true;

            // Contains classes in the System namespace.
            foreach (ModuleDefinition m in assembly.Modules)
            {
                foreach (TypeDefinition t in m.GetTypes())
                {
                    if (t.Namespace.StartsWith("System"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the public key token out of the given assembly name.
        /// </summary>
        private static string GetPublicKeyToken(AssemblyNameReference name)
        {
            if (!name.HasPublicKey)
                return string.Empty;
            var token = string.Join("", name.PublicKeyToken.Select(x => x.ToString("x2")));
            return token;
        }
    }
}
