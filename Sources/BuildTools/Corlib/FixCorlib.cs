using System.IO;
using Dot42.BuildTools.Exceptions;
using Mono.Cecil;

namespace Dot42.BuildTools.Corlib
{
    /// <summary>
    /// Fix the assembly name of mscorlib.dll
    /// </summary>
    internal static class FixCorlib
    {
        /// <summary>
        /// Fix the given mscorlib.dll assembly.
        /// </summary>
        internal static void Fix(string assemblyPath)
        {
            AssemblyDefinition assembly;
            var tmpFilePath = assemblyPath + ".tmp";

            using (var resolver = new AssemblyResolver(new[] {Path.GetDirectoryName(assemblyPath)}))
            {
                assembly = resolver.Resolve(Path.GetFileNameWithoutExtension(assemblyPath), new ReaderParameters(ReadingMode.Immediate) {ReadSymbols = false});
                assembly.Name.Name = "mscorlib";
                assembly.Write(tmpFilePath);
            }
            
            File.Copy(tmpFilePath, assemblyPath, true);
            File.Delete(tmpFilePath);
        }
    }
}
