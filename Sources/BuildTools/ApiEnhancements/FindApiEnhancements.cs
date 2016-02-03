using System.IO;
using Dot42.BuildTools.Exceptions;
using Mono.Cecil;

namespace Dot42.BuildTools.ApiEnhancements
{
    /// <summary>
    /// Find all places where the API can be enhanced.
    /// </summary>
    internal static class FindApiEnhancements
    {
        /// <summary>
        /// Find all API enhancements
        /// </summary>
        internal static void Find(string assemblyPath)
        {
            var resolver = new AssemblyResolver(new[] { Path.GetDirectoryName(assemblyPath) });
            var assembly = resolver.Resolve(Path.GetFileNameWithoutExtension(assemblyPath), 
                new ReaderParameters(ReadingMode.Immediate) { ReadSymbols = false, AssemblyResolver = resolver});

            FindListenerInterfaces.Find(assembly);
            FindRunnableArguments.Find(assembly);
        }
    }
}
