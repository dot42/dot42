using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.BuildTools.Exceptions;
using Mono.Cecil;

namespace Dot42.BuildTools.CheckForwarders
{
    /// <summary>
    /// Find types added to dot42.dll, missing from forward assemblies.
    /// </summary>
    internal static class CheckForwardAssemblies
    {
        /// <summary>
        /// Check the given dot42.dll wrt forward assemblies.
        /// </summary>
        internal static void Check(string assemblyPath)
        {
            // Load dot42.dll
            var resolver = new AssemblyResolver(new[] { Path.GetDirectoryName(assemblyPath) }) { ShowLoading = false };
            var dot42 = resolver.Resolve(Path.GetFileNameWithoutExtension(assemblyPath), new ReaderParameters(ReadingMode.Immediate) { ReadSymbols = false });

            // Load forward assemblies
            var fwAssemblies = new List<AssemblyDefinition>();
            var folder = Path.GetDirectoryName(assemblyPath);
            foreach (var path in Directory.GetFiles(folder, "*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, dot42.Name.Name, StringComparison.OrdinalIgnoreCase))
                    continue;

                var fwAssembly = resolver.Resolve(name, new ReaderParameters(ReadingMode.Immediate) {ReadSymbols = false});
                fwAssemblies.Add(fwAssembly);
            }

            var existingCount = 0;
            var missingCount = 0;
            // Go over all types
            foreach (var type in dot42.MainModule.Types)
            {
                if (!type.IsPublic)
                    continue;
                if (!type.Namespace.StartsWith("System"))
                    continue;
                if (HasForwarder(type, fwAssemblies))
                {
                    existingCount++;
                    continue;
                }

                Console.WriteLine("Missing forwarder for {0}", type.FullName);
                missingCount++;
            }

            Console.WriteLine("{0} forwarders missing, {1} found.", missingCount, existingCount);
        }

        /// <summary>
        /// Is there a forwarder for the given type?
        /// </summary>
        private static bool HasForwarder(TypeDefinition type, List<AssemblyDefinition> fwAssemblies)
        {
            var fullName = type.FullName;
            foreach (var assembly in fwAssemblies)
            {
                if (assembly.MainModule.ExportedTypes.Any(x => x.FullName == fullName))
                    return true;
            }
            return false;
        }
    }
}
