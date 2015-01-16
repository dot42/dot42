using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Dot42.VStudio;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.TemplateBuilder
{
    internal class TemplateBuilder
    {
        private const string NamePrefix = "Dot42";
        private const string Lcid = "1033";
        private readonly string sourceFolder;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TemplateBuilder(string sourceFolder)
        {
            this.sourceFolder = sourceFolder;
        }

        /// <summary>
        /// Build the template and save the resulting ZIP in the given folder.
        /// </summary>
        public void Build(string targetFolder)
        {
            // Get template name
            var name = NamePrefix + Path.GetFileName(sourceFolder);

            // Load the template file
            var template = XDocument.Parse(LoadVsTemplate());
            var ns = template.Root.Name.NamespaceName;
            var projectType = template.Descendants(XName.Get("ProjectType", ns)).Single().Value;

            // Update wizard assembly
            foreach (var wizardE in template.Root.Descendants(XName.Get("WizardExtension", ns)))
            {
                var assemblyE = wizardE.Element(XName.Get("Assembly", ns));
                if ((assemblyE != null) && string.IsNullOrEmpty(assemblyE.Value))
                {
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    assemblyE.Value = string.Format("Dot42.VStudio10.Editors, Version={0}, Culture=neutral, PublicKeyToken=0a72796057571e65", version);
                }
            }
            var templateContent = template.ToString();

            // Ensure target folder exists
            var folder = Path.Combine(Path.Combine(Path.Combine(targetFolder, projectType), NamePrefix), Lcid);
            Directory.CreateDirectory(folder);

            // Create ZIP file.
            var path = Path.Combine(folder, name + ".zip");
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var zipStream = new ZipOutputStream(fileStream) {UseZip64 = UseZip64.Off})
                {
                    zipStream.SetLevel(9);

                    // Add template
                    zipStream.PutNextEntry(new ZipEntry(name + ".vstemplate"));
                    var templateData = Encoding.UTF8.GetBytes(templateContent);
                    zipStream.Write(templateData, 0, templateData.Length);
                    zipStream.CloseEntry();

                    // Add remaining files
                    CopyFilesToZip(sourceFolder, string.Empty, zipStream);
                }
            }
        }

        /// <summary>
        /// Copy all files (except vstemplate) in the given folder and sub-folders to the given zip.
        /// </summary>
        private static void CopyFilesToZip(string folder, string zipFolder, ZipOutputStream zipStream)
        {
            // Files
            foreach (var file in Directory.GetFiles(folder).Where(x => !x.EndsWith(".vstemplate")))
            {
                zipStream.PutNextEntry(new ZipEntry(zipFolder + Path.GetFileName(file)));
                var data = File.ReadAllBytes(file);
                zipStream.Write(data, 0, data.Length);
                zipStream.CloseEntry();                
            }

            // Sub folders
            foreach (var subFolder in Directory.GetDirectories(folder))
            {
                CopyFilesToZip(subFolder, zipFolder + "/" + Path.GetFileName(subFolder), zipStream);                
            }            
        }

        /// <summary>
        /// Load the .vstemplate file
        /// </summary>
        private string LoadVsTemplate()
        {
            var path = Directory.GetFiles(sourceFolder).Single(x => x.EndsWith(".vstemplate"));
            var contents = File.ReadAllText(path);

            // Make replacements
            contents = contents.Replace("{package-guid}", "{" + GuidList.Strings.guidDot42Package + "}");

            return contents;
        }
    }
}
