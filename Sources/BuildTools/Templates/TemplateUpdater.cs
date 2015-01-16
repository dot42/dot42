using System;
using System.IO;
using System.Xml.Linq;

namespace Dot42.BuildTools.Templates
{
    internal static class TemplateUpdater
    {
        /// <summary>
        /// Update the assembly in the given template
        /// </summary>
        internal static void UpdateTemplate(string path, string target)
        {
            // Load document
            var doc = XDocument.Load(path);

            // Update wizard assembly elements
            const string assemblyTemplate = "dot42.VStudio.Project.{0}, Version={1}, Culture=neutral, PublicKeyToken=0a72796057571e65";
            var assemblyValue = string.Format(assemblyTemplate, target, typeof(Program).Assembly.GetName().Version);

            var changed = false;
            foreach (var assembly in doc.Descendants(XName.Get("Assembly", doc.Root.Name.NamespaceName)))
            {
                if (assembly.Value != assemblyValue)
                {
                    assembly.Value = assemblyValue;
                    changed = true;
                }
            }
            if (!changed)
                return;

            // Save changes
            doc.Save(path);
            Console.WriteLine("Updated {0}", Path.GetFileName(path));
        }
    }
}
