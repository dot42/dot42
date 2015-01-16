using System.IO;
using System.Linq;
using Dot42.BuildTools.Exceptions;

namespace Dot42.BuildTools.EnumNames
{
    /// <summary>
    /// Build source code for enum value names
    /// </summary>
    internal static class EnumNamesBuilder
    {
        private const string Template = @"
using System;
namespace $ns$
{
    partial class $name$Names
    {
        internal static readonly Tuple<int, string>[] Names = new[] { $values$ };
    }
}";

        /// <summary>
        /// Generate the enums
        /// </summary>
        internal static void Generate(string assemblyPath, string outputFolder, string[] enumTypeNames)
        {
            var resolver = new AssemblyResolver(new[] { Path.GetDirectoryName(assemblyPath) });
            var assembly = resolver.Resolve(Path.GetFileNameWithoutExtension(assemblyPath));

            foreach (var name in enumTypeNames)
            {
                var type = assembly.MainModule.GetType(name);
                var values = type.Fields.Where(x => x.IsStatic).Select(x => string.Format("Tuple.Create({0}, \"{1}\")", (int)x.Constant, x.Name));

                var src = Template;
                src = src.Replace("$ns$", type.Namespace);
                src = src.Replace("$name$", type.Name);
                src = src.Replace("$values$", string.Join(",\n", values));

                var outputPath = Path.Combine(outputFolder, type.Name + ".cs");
                var existing = File.Exists(outputPath) ? File.ReadAllText(outputPath) : string.Empty;
                if (src != existing)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    File.WriteAllText(outputPath, src);
                }
            }
        }
    }
}
