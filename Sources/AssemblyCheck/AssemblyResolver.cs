using System.IO;
using Mono.Cecil;

namespace Dot42.AssemblyCheck
{
    internal class AssemblyResolver : IAssemblyResolver
    {
        private static readonly string[] Extensions = new[] { ".dll", ".exe" };
        private readonly string localFolder;
        private readonly string frameworkFolder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AssemblyResolver(string localFolder, string frameworkFolder)
        {
            this.localFolder = localFolder;
            this.frameworkFolder = frameworkFolder;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            var assembly = ResolveInFolder(localFolder, name, parameters);
            if (assembly != null) return assembly;
            assembly = ResolveInFolder(frameworkFolder, name, parameters);
            if (assembly != null) return assembly;
            throw new AssemblyResolutionException(name);
        }

        /// <summary>
        /// Try to resolve the assembly in the given folder.
        /// </summary>
        /// <returns>Null if not found.</returns>
        private AssemblyDefinition ResolveInFolder(string folder, AssemblyNameReference name, ReaderParameters parameters)
        {
            var basePath = Path.Combine(folder, name.Name);
            foreach (var ext in Extensions)
            {
                var path = basePath + ext;
                if (File.Exists(path))
                    return AssemblyDefinition.ReadAssembly(path, parameters);
            }
            if (!File.Exists(basePath))
                return null;
            return AssemblyDefinition.ReadAssembly(basePath, parameters);            
        }

        AssemblyDefinition IAssemblyResolver.Resolve(AssemblyNameReference name)
        {
            return Resolve(name, CreateReaderParameters());
        }

        //AssemblyDefinition IAssemblyResolver.Resolve(string fullName)
        //{
        //    return Resolve(AssemblyNameReference.Parse(fullName), CreateReaderParameters());
        //}

        //AssemblyDefinition IAssemblyResolver.Resolve(string fullName, ReaderParameters parameters)
        //{
        //    return Resolve(AssemblyNameReference.Parse(fullName), parameters);
        //}

        private ReaderParameters CreateReaderParameters()
        {
            return new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = this };
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion
    }
}
