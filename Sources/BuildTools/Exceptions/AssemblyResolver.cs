using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.BuildTools.Exceptions
{
    /// <summary>
    /// Resolve assemblies.
    /// </summary>
    internal class AssemblyResolver : IAssemblyResolver
    {
        private readonly List<string> referenceFolders;
        private readonly Dictionary<string, AssemblyDefinition> references = new Dictionary<string, AssemblyDefinition>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AssemblyResolver(IEnumerable<string> referenceFolders)
        {
            this.referenceFolders = referenceFolders.Select(ToFolder).Distinct().ToList();
            ShowLoading = true;
        }

        /// <summary>
        /// Write loading messages to the console
        /// </summary>
        public bool ShowLoading { get; set; }

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
        /// Resolve an assembly by name.
        /// </summary>
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, new ReaderParameters(ReadingMode.Deferred) {
                AssemblyResolver = this,
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

            var referencePaths = referenceFolders.Select(x => Path.Combine(x, name.Name + ".dll"));
            var path = referencePaths.FirstOrDefault(x => string.Equals(Path.GetFileNameWithoutExtension(x), name.Name, StringComparison.OrdinalIgnoreCase) && File.Exists(x));
            if (path == null)
            {
                DLog.Error(DContext.CompilerAssemblyResolver, "Failed to resolve assembly {0} in {1}", name, string.Join(";  ", referenceFolders));
                throw new AssemblyResolutionException(name);
            }

            try
            {
                if (ShowLoading)
                    Console.WriteLine(string.Format("Loading {0}", name.Name));
                var reference = AssemblyDefinition.ReadAssembly(path, parameters);
                references[name.Name] = reference;
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
    }
}
