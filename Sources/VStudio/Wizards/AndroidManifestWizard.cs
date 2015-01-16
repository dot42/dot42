using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

using Dot42.FrameworkDefinitions;
using Dot42.Ide.WizardForms;

using EnvDTE;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;

using VSLangProj;

namespace Dot42.VStudio.Wizards
{
    /// <summary>
    /// Wizard used in manifest creation
    /// </summary>
    public class AndroidManifestWizard : IWizard
    {
        #region IWizard implementation

        /// <summary>
        /// Runs custom wizard logic at the beginning of a template wizard run.
        /// </summary>
        /// <param name="automationObject">The automation object being used by the template wizard.</param><param name="replacementsDictionary">The list of standard parameters to be replaced.</param><param name="runKind">A <see cref="T:Microsoft.VisualStudio.TemplateWizard.WizardRunKind"/> indicating the type of wizard run.</param><param name="customParams">The custom parameters with which to perform parameter replacement in the project.</param>
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            DTE dte = automationObject as DTE;
            object[] currentProject = dte.ActiveSolutionProjects as object[];
            Project cp = currentProject[0] as Project;

            XElement root = XElement.Load(cp.FullName);

            string minSdk = AndroidManifestWizard.FormatTargetSdkVersion(GetElementContent(root, "TargetFrameworkVersion"));
            string targetSdk = AndroidManifestWizard.FormatTargetSdkVersion(GetElementContent(root, "TargetSdkAndroidVersion"));
            string package = GetElementContent(root, "PackageName");
            string assemblyName = GetElementContent(root, "AssemblyName");

            if (string.IsNullOrEmpty(targetSdk))
            {
                targetSdk = minSdk;
            }

            replacementsDictionary.Add("$minSdkVersion$", minSdk);
            replacementsDictionary.Add("$targetSdkVersion$", targetSdk);
            replacementsDictionary.Add("$packagename$", package);
            replacementsDictionary.Add("$assemblyname$", assemblyName);
        }

        private static string GetElementContent(XElement root, string element)
        {
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            IEnumerable<string> minSdkVersion =
                from segment in root.Descendants(ns + element)
                select (string)segment;
            if (1 == minSdkVersion.Count<string>())
            {
                return minSdkVersion.First<string>();
            }

            return string.Empty;
        }

        /// <summary>
        /// Indicates whether the specified project item should be added to the project.
        /// </summary>
        /// <returns>
        /// true if the project item should be added to the project; otherwise, false.
        /// </returns>
        /// <param name="filePath">The path to the project item.</param>
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        /// <summary>
        /// Runs custom wizard logic before opening an item in the template.
        /// </summary>
        /// <param name="projectItem">The project item that will be opened.</param>
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        /// <summary>
        /// Runs custom wizard logic when a project item has finished generating.
        /// </summary>
        /// <param name="projectItem">The project item that finished generating.</param>
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        /// <summary>
        /// Runs custom wizard logic when a project has finished generating.
        /// </summary>
        /// <param name="project">The project that finished generating.</param>
        public void ProjectFinishedGenerating(Project project)
        {
        }

        /// <summary>
        /// Runs custom wizard logic when the wizard has completed all tasks.
        /// </summary>
        public void RunFinished()
        {
        }

        #endregion IWizard implementation

        #region private methods

        /// <summary>
        /// Format a value specified in PackageAttribute.TargetSdkVersion towards an API level.
        /// </summary>
        private static string FormatTargetSdkVersion(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            int result;

            // Try api level first
            if (int.TryParse(value, out result))
                return result.ToString();

            // Match against known framework versions
            var framework = Frameworks.Instance.FirstOrDefault(x => x.Name == value);
            if (framework != null)
            {
                return framework.Descriptor.ApiLevel.ToString();
            }

            return string.Empty;
        }

        #endregion private methods
    }
}
