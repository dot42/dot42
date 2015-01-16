using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.Gui.SamplesTool
{
    internal static class SampleProjectUpdater
    {
        private const string ElementName = "ApkCertificateThumbprint";

        /// <summary>
        /// Update all sample projects.
        /// </summary>
        /// <returns>True on success, false on errors</returns>
        internal static bool UpdateSampleProjects(string folder, string thumbprint, Action<string> log)
        {
            var ok = true;

            //log(string.Format("Processing {0}", folder));
            foreach (var project in Directory.GetFiles(folder, "*.csproj"))
            {
                if (!UpdateProject(project, thumbprint, log))
                {
                    ok = false;
                }
            }

            // Nested folders
            foreach (var subFolder in Directory.GetDirectories(folder))
            {
                if (!UpdateSampleProjects(subFolder, thumbprint, log))
                {
                    ok = false;
                }
            }
            return ok;
        }

        /// <summary>
        /// Update the given sample projects.
        /// </summary>
        private static bool UpdateProject(string path, string thumbprint, Action<string> log)
        {
            try
            {
                var doc = XDocument.Load(path);
                var modified = false;
                foreach (var node in doc.Root.Descendants().Where(x => x.Name.LocalName == ElementName))
                {
                    node.Value = thumbprint;
                    modified = true;
                }
                if (modified)
                {
                    doc.Save(path);
                    log(string.Format("Updated {0}", path));
                }
                return true;
            }
            catch (Exception ex)
            {
                log(string.Format("Failed to update {0} because {1}", path, ex.Message));
                return false;
            }
        }
    }
}
