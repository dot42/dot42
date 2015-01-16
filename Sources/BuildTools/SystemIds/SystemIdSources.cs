using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.BuildTools.SystemIds
{
    internal static class SystemIdSources
    {
        private const string Template = @"namespace Dot42.FrameworkDefinitions
{
    /// <summary>
    /// Set of ID names used by the Dot42 SDK.
    /// </summary>
    public static class SystemIdConstants
    {
        public static readonly string[] Ids = new[] {
$$$
        };
    }
}";

        /// <summary>
        /// Generate the script
        /// </summary>
        internal static void Generate(string systemIdSourcePath, string frameworksFolder)
        {
            // Get latest framework folder
            Frameworks frameworks = new Frameworks(frameworksFolder);
            string frameworkFolder = frameworks.GetNewestVersion().Folder;

            List<string> list = CollectIds(frameworkFolder);
            string ids = string.Join(",\r\n", list.Select(x => string.Format("            \"{0}\"", x)));

            string script = Template.Replace("$$$", ids);
            string existing = File.Exists(systemIdSourcePath) ? File.ReadAllText(systemIdSourcePath) : string.Empty;
            if (script != existing)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(systemIdSourcePath));
                File.WriteAllText(systemIdSourcePath, script);
            }
        }

        private static List<string> CollectIds(string frameworkFolder)
        {
            var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(frameworkFolder, "Dot42.dll"));
            var list = new List<string>();
            foreach (var type in assembly.MainModule.Types)
            {
                CollectIds(type, list);
            }
            return list.Distinct().OrderBy(x => x).ToList();
        }

        private static void CollectIds(TypeDefinition type, List<string> ids)
        {
            foreach (var field in type.Fields)
            {
                if (!field.HasCustomAttributes)
                    continue;
                var attr = field.CustomAttributes.FirstOrDefault(x =>
                        (x.AttributeType.Name == AttributeConstants.ResourceIdAttributeName) &&
                        (x.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace));
                if (attr == null)
                    continue;
                ids.Add((string) attr.ConstructorArguments[0].Value);
            }

            foreach (var nested in type.NestedTypes)
            {
                CollectIds(nested, ids);
            }
        }
    }
}
