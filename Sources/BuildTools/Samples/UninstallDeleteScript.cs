using System.IO;
using System.Text;

namespace Dot42.BuildTools.Samples
{
    internal static class UninstallDeleteScript
    {
        /// <summary>
        /// Generate the script
        /// </summary>
        internal static void Generate(string scriptPath, string samplesFolder)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[UninstallDelete]");

            samplesFolder = Path.GetFullPath(samplesFolder);
            AddFolder(sb, samplesFolder, samplesFolder);

            var existing = File.Exists(scriptPath) ? File.ReadAllText(scriptPath) : string.Empty;
            var script = sb.ToString();
            if (script != existing)
            {
                File.WriteAllText(scriptPath, script);
            }
        }

        private static void AddFolder(StringBuilder sb, string folder, string root)
        {
            if (Directory.GetFiles(folder, "*.csproj").Length > 0)
            {
                var subPath = Path.GetFullPath(folder).Substring(root.Length);
                if (subPath.StartsWith("\\"))
                    subPath = subPath.Substring(1);

                // bin folder
                sb.Append("Type: filesandordirs; Name: \"{userdocs}\\Dot42\\Samples\\");
                sb.Append(subPath);
                sb.AppendLine("\\bin\";");
                // obj folder
                sb.Append("Type: filesandordirs; Name: \"{userdocs}\\Dot42\\Samples\\");
                sb.Append(subPath);
                sb.AppendLine("\\obj\";");
                // project.suo
                sb.Append("Type: files; Name: \"{userdocs}\\Dot42\\Samples\\");
                sb.Append(subPath);
                sb.AppendLine("\\*.suo\";");
            }
            
            foreach (var child in Directory.GetDirectories(folder))
            {
                AddFolder(sb, child, root);
            }
        }
    }
}
