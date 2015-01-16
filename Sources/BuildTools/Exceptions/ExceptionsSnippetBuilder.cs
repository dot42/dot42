using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.BuildTools.Exceptions
{
    internal static class ExceptionsSnippetBuilder
    {
        private const string NsTemplate = "        ProvideDebugException(GuidList.Strings.guidDot42DebuggerId, ExceptionConstants.NamespaceLevelCode, ExceptionConstants.TopLevelName, \"{0}\"),";
        private const string TypeTemplate = "        ProvideDebugException(GuidList.Strings.guidDot42DebuggerId, ExceptionConstants.NamespaceLevelCode, ExceptionConstants.TopLevelName, \"{0}\", \"{1}\"),";

        private const string Template = @"using Microsoft.VisualStudio.Shell;
using Dot42.VStudio.Debugger;

namespace Dot42.VStudio
{
    [
$$$
    ]
    partial class Dot42Package 
    {
    }
}";

        /// <summary>
        /// Generate the script
        /// </summary>
        internal static void Generate(string snippetSourcePath, string frameworksFolder)
        {
            // Get latest framework folder
            Frameworks frameworks = new Frameworks(frameworksFolder);
            string frameworkFolder = frameworks.GetNewestVersion().Folder;

            List<TypeDefinition> list = CollectExceptions(frameworkFolder);
            List<string> namespaces = list.Select(x => x.Namespace).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            List<string> lines = new List<string>();

            foreach (string ns in namespaces)
            {
                lines.Add(string.Format(NsTemplate, ns));
            }

            foreach (TypeDefinition type in list)
            {
                lines.Add(string.Format(TypeTemplate, type.Namespace, type.FullName));
            }

            string allLines = string.Join(Environment.NewLine, lines);
            allLines = allLines.Substring(0, allLines.Length - 1); // strip last ','
            string script = Template.Replace("$$$", allLines);
            string existing = File.Exists(snippetSourcePath) ? File.ReadAllText(snippetSourcePath) : string.Empty;
            if (script != existing)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(snippetSourcePath));
                File.WriteAllText(snippetSourcePath, script);
            }
        }

        private static List<TypeDefinition> CollectExceptions(string frameworkFolder)
        {
            var resolver = new AssemblyResolver(new[] { frameworkFolder });
            var assembly = resolver.Resolve("Dot42");
            var list = new List<TypeDefinition>();
            foreach (var type in assembly.MainModule.Types)
            {
                CollectExceptions(type, list);
            }
            return list.Distinct().OrderBy(x => x.FullName).ToList();
        }

        private static void CollectExceptions(TypeDefinition type, List<TypeDefinition> exceptions)
        {
            if (IsException(type))
            {
                exceptions.Add(type);
            }

            /*foreach (var nested in type.NestedTypes)
            {
                CollectExceptions(nested, exceptions);
            }*/
        }

        private static bool IsException(TypeDefinition type)
        {
            if (type.FullName == "System.Exception")
                return true;
            var baseType = type.BaseType;
            if (baseType == null)
                return false;
            return IsException(baseType.Resolve());
        }
    }
}
