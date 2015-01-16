using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dot42.Ide.WizardForms;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace Dot42.VStudio.Wizards
{
    /// <summary>
    /// Wizard used in project creation
    /// </summary>
    public class ClassLibraryProjectWizard : IWizard
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ClassLibraryProjectWizard()
        {
            Dot42Package.InitLocations();
            TargetFrameworkVersion = string.Empty;
        }

        /// <summary>
        /// Version of android target.
        /// </summary>
        private string TargetFrameworkVersion { get; set; }

        /// <summary>
        /// Runs custom wizard logic at the beginning of a template wizard run.
        /// </summary>
        /// <param name="automationObject">The automation object being used by the template wizard.</param><param name="replacementsDictionary">The list of standard parameters to be replaced.</param><param name="runKind">A <see cref="T:Microsoft.VisualStudio.TemplateWizard.WizardRunKind"/> indicating the type of wizard run.</param><param name="customParams">The custom parameters with which to perform parameter replacement in the project.</param>
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            using (var dialog = new ClassLibraryProjectWizardDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TargetFrameworkVersion = dialog.TargetFrameworkVersion;
                }
                else
                {
                    // About 
                    throw new WizardCancelledException("Wizard cancelled.");
                }
            }

            replacementsDictionary.Add("$ProjectTypeGuid$", ProjectConstants.ProjectTypeGuid);
            replacementsDictionary.Add("$Target$", ProjectConstants.TargetValue);
            replacementsDictionary.Add("$PackageExt$", ProjectConstants.PackagExt);
            replacementsDictionary.Add("$PackageFileNameTag$", ProjectConstants.PackageFileNameTag);
            replacementsDictionary.Add("$TargetFrameworkVersion$", TargetFrameworkVersion);
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
        /// Runs custom wizard logic when the wizard has completed all tasks.
        /// </summary>
        public void RunFinished()
        {
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
    }
}
