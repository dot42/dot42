using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly List<string> referenceFolders;
        private readonly ConcurrentDictionary<string, AssemblyDefinition> referencesByName = new ConcurrentDictionary<string, AssemblyDefinition>();
        private readonly ConcurrentDictionary<AssemblyDefinition, string> fileNamesByAssembly = new ConcurrentDictionary<AssemblyDefinition, string>();

        private readonly AssemblyClassLoader classLoader;
        private readonly Action<AssemblyDefinition> assemblyLoaded;

        // This is used to prevent loading of multiple assemblies at the same time. keys can be assemby names
        // or filenames. Note that filenames ignore casing.
        private readonly ConcurrentDictionary<string, Task<AssemblyDefinition>> loadingTasks =
                            new ConcurrentDictionary<string, Task<AssemblyDefinition>>(StringComparer.InvariantCultureIgnoreCase);
        
        /// <summary>
        /// For all explicitly and implicitly loaded assemblies the classloader 
        /// and the callback will be invoked, if not null. Callback and classloader 
        /// might be called reentrantly, if the AssemblyResolver itself is used reentrantly.
        /// </summary>
        public AssemblyResolver(IEnumerable<string> referenceFolders, AssemblyClassLoader classLoader, Action<AssemblyDefinition> assemblyLoaded)
        {
            this.classLoader = classLoader;
            this.assemblyLoaded = assemblyLoaded;
            this.referenceFolders = referenceFolders.Select(ToFolder).Distinct().ToList();
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

            return Load(null, fullPath, parameter);
        }

        /// <summary>
        /// this is fully reentrant and multithreading capable, but will itself block.
        /// </summary>
        private AssemblyDefinition Load(AssemblyNameReference name, string assemblyFilename, ReaderParameters parameters)
        {
            AssemblyDefinition ret = null;
            TaskCompletionSource<AssemblyDefinition> loadingTaskSource = new TaskCompletionSource<AssemblyDefinition>();

            bool nameRegistered = false, filenameRegistered = false;

            try
            {
                // First, make sure we are the only one loading.
                while (true)
                {
                    Task<AssemblyDefinition> loadingTask;
                    if (name != null && !nameRegistered)
                    {
                        if (loadingTasks.TryGetValue(name.Name, out loadingTask))
                        {
                            ret = loadingTask.Result;
                            return ret;
                        }
                            
                        if (!loadingTasks.TryAdd(name.Name, loadingTaskSource.Task))
                            continue;
                        nameRegistered = true;
                    }

                    if (loadingTasks.TryAdd(assemblyFilename, loadingTaskSource.Task))
                    {
                        filenameRegistered = true;
                        break;
                    }
                    if (loadingTasks.TryGetValue(assemblyFilename, out loadingTask))
                    {
                        ret = loadingTask.Result;
                        return ret;
                    }
                }

                // now check if it has already been loaded.
                if (name != null)
                {
                    if (referencesByName.TryGetValue(name.Name, out ret))
                        return ret;
                }

                ret = fileNamesByAssembly.FirstOrDefault(v => v.Value.Equals(assemblyFilename, StringComparison.InvariantCultureIgnoreCase)).Key;
                if (ret != null)
                    return ret;

                // now load the assembly.
                Console.WriteLine("Loading {0}...", Path.GetFileName(assemblyFilename));

                AssemblyDefinition assm = AssemblyDefinition.ReadAssembly(assemblyFilename, parameters);    

                VerifyFrameworkAssembly(assm, assemblyFilename);

                // have to use a lock to update both data structures at the same time.
                lock (referencesByName)
                {
                    // now check again by the assembly name if it has been loaded before.
                    // This can happen if we were only provided a file name
                    if (!referencesByName.TryAdd(assm.Name.Name, assm))
                    {
                        ret = referencesByName[assm.Name.Name];
                        return ret;
                    }

                    fileNamesByAssembly[assm] = assemblyFilename;
                }

                ret = assm;

                // now notify any listeners. note that if we were called on multiple threads
                // these might be called reentrantly as well.

                if(assemblyLoaded != null)
                    assemblyLoaded(ret);

                if (classLoader != null)
                    classLoader.LoadAssembly(ret);

                // done.
                return ret;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
#endif
                // Log the error
                DLog.Error(DContext.CompilerAssemblyResolver, "Failed to load assembly {0}", ex, name);

                // Pass the error on
                var exn = new AssemblyResolutionException(name);
                loadingTaskSource.SetException(exn);
                throw exn;
            }
            finally
            {
                loadingTaskSource.TrySetResult(ret);
                
                // cleanup our registrations
                Task<AssemblyDefinition> ignore;
                if (nameRegistered)
                    loadingTasks.TryRemove(name.Name, out ignore);
                if (filenameRegistered)
                    loadingTasks.TryRemove(assemblyFilename, out ignore);
            }
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
            if (referencesByName.TryGetValue(name.Name, out result))
                return result;

            var path = ResolvePath(name.Name);
            if (path == null)
            {
                DLog.Error(DContext.CompilerAssemblyResolver, "Failed to resolve assembly {0} in {1}", name, string.Join(";  ", referenceFolders));
                throw new AssemblyResolutionException(name);
            }

            return Load(name, path, parameters);
        }

        /// <summary>
        /// returns the filename of the assembly definition, or null if none.
        /// </summary>
        public string GetFileName(AssemblyDefinition def)
        {
            if(fileNamesByAssembly.ContainsKey(def))
                return fileNamesByAssembly[def];
            return null;
        }

        /// <summary>
        /// Resolve the given name into a path.
        /// </summary>
        public string ResolvePath(string path)
        {
            if (File.Exists(path))
                return path;
            var name = Path.GetFileName(path);
            var referencePaths = referenceFolders.Select(x => Path.Combine(x, name + ".dll"))
                         .Concat(referenceFolders.Select(x => Path.Combine(x, name)))
                         .Concat(referenceFolders.Select(x => Path.Combine(x, name + ".exe")));

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
        /// Is the given path is a file, return it's folder, otherwise return the given path.
        /// </summary>
        private static string ToFolder(string path)
        {
            if (File.Exists(path))
                return Path.GetDirectoryName(path);
            return path;
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
