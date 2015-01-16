
using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop
{
	/// <summary>
	/// Description of SetupDot42ProjectCommand.
	/// </summary>
	public class SetupDot42ProjectCommand : AbstractCommand
	{
		public SetupDot42ProjectCommand() {
			Dot42Addin.InitializeLocations();
		}
		
		public override void Run()
		{
			if (!(Owner is MSBuildBasedProject))
				throw new ArgumentException("Project must be an MSBuild based project.");
			var project = (MSBuildBasedProject)Owner;
			project.PerformUpdateOnProjectFile(() => SetupProject(project));
		}
		
		private void SetupProject(MSBuildBasedProject project)
		{
			var projectFile = project.MSBuildProjectFile;
			
			// Fix project type
			projectFile.AddProperty("ProjectTypeGuids", "{337B7DB7-2D1E-448D-BEBF-17E887A46E37};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}");
			
			// Set property values
			projectFile.AddProperty("TargetFrameworkVersion", ProjectCreateData.TargetFrameworkVersion);
			projectFile.AddProperty("AndroidVersion", "$(TargetFrameworkVersion)");
			projectFile.AddProperty("ApkFilename", project.Name + ".apk");
			projectFile.AddProperty("PackageName", "com." + project.Name);
			projectFile.AddProperty("ApkCertificatePath", ProjectCreateData.CertificatePath);
			projectFile.AddProperty("ApkCertificateThumbprint", ProjectCreateData.CertificateThumbprint);
			
			// Add Dot42ExtensionPath properties
            projectFile.AddProperty("Dot42ExtensionsPath", @"$(Registry:HKEY_CURRENT_USER\SOFTWARE\TallComponents\Dot42@ExtensionsPath)").Condition = " '$(Dot42ExtensionsPath)' == '' ";
            projectFile.AddProperty("Dot42ExtensionsPath", @"$(Registry:HKEY_LOCAL_MACHINE\SOFTWARE\TallComponents\Dot42@ExtensionsPath)").Condition = " '$(Dot42ExtensionsPath)' == '' ";
			
			// Add dot42 Import
			projectFile.AddImport(@"$(Dot42ExtensionsPath)\Dot42.CSharp.targets");
			
			// Add R.cs
			projectFile.AddItem("Compile", "$(ResourcesGeneratedCodePath)R.cs");
			
			// Add additional library
			foreach (var name in ProjectCreateData.AdditionalLibraryNames)
			{
				projectFile.AddItem("Reference", name);
			}
		}
	}
}
