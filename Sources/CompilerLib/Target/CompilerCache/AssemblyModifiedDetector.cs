using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.CompilerLib.Target.CompilerCache
{
    /// <summary>
    /// Allows to check if an assembly was modified since the last build
    /// (as identified through the map file). Uses an internal cache.
    /// </summary>
    public class AssemblyModifiedDetector
    {
        private readonly Func<AssemblyDefinition, string> _filenameFromAssembly;
        private readonly MapFileLookup _map;
        private readonly ConcurrentDictionary<AssemblyDefinition, bool> _modified = new ConcurrentDictionary<AssemblyDefinition, bool>();

        public AssemblyModifiedDetector(Func<AssemblyDefinition, string> filenameFromAssembly, MapFileLookup map)
        {
            _filenameFromAssembly = filenameFromAssembly;
            _map = map;
        }

        public bool IsModified(AssemblyDefinition assembly)
        {
            bool isModified;
            if (_modified.TryGetValue(assembly, out isModified))
                return isModified;

            var assemblyFileName = _filenameFromAssembly(assembly);
            if (assemblyFileName == null)
            {
                _modified[assembly] = true;
                return true;
            }
                
            var scopeEntry = _map.Scopes.FirstOrDefault(s => s.Filename.Equals(assemblyFileName, StringComparison.Ordinal));

            if (scopeEntry == null)
            {
                _modified[assembly] = true;
                return true;
            }

            isModified = File.GetLastWriteTimeUtc(assemblyFileName) != scopeEntry.Timestamp
                        || Hash.HashFileMD5(assemblyFileName) != scopeEntry.Hashcode;
            _modified[assembly] = isModified;
            return isModified;
        }
    }
}
