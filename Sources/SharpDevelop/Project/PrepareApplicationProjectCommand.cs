
using System;
using System.Windows.Forms;
using Dot42.Ide.WizardForms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.Exceptions;

namespace Dot42.SharpDevelop
{
	/// <summary>
	/// Initialize an application project.
	/// </summary>
	public class PrepareApplicationProjectCommand : AbstractCommand
	{
		public PrepareApplicationProjectCommand() {
			Dot42Addin.InitializeLocations();
		}
		
		public override void Run()
		{
			var creationInfo = Owner as ProjectCreateInformation;
			if (creationInfo == null)
				throw new ArgumentException("Owner must be a ProjectCreateInformation.");

			using (var dialog = new ApplicationProjectWizardDialog()) {
				if (dialog.ShowDialog() != DialogResult.OK)
					throw new InvalidProjectFileException("Project creation cancelled");
				
				// Save settings
				ProjectCreateData.CertificatePath = dialog.CertificatePath;
				ProjectCreateData.CertificateThumbprint = dialog.CertificateThumbprint;
				ProjectCreateData.TargetFrameworkVersion = dialog.TargetFrameworkVersion;
				ProjectCreateData.AdditionalLibraryNames = dialog.AdditionalLibraryNames;
			}
		}
	}
}
