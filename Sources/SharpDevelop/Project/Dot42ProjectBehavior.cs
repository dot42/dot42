using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dot42.Ide.Debugger;
using Dot42.Ide.Project;
using Dot42.SharpDevelop.Project;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using Dot42.DeviceLib.UI;
using Dot42.ApkLib;
using Dot42.FrameworkDefinitions;

namespace Dot42.SharpDevelop
{
	/// <summary>
	/// Description of Dot42ProjectBehavior.
	/// </summary>
	public class Dot42ProjectBehavior : ProjectBehavior
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public Dot42ProjectBehavior()
		{
			Dot42Addin.FixAddinTreeConditions();
			Dot42Addin.InitializeLocations();
		}
		
		public override bool IsStartable {
			get { return true; }
		}
		
		public override void Start(bool withDebugging)
		{
			// Open the package
			ApkFile apkFile;
			var apkPath = ApkPath;
			try
			{
				apkFile = new ApkFile(apkPath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format("Failed to open package because: {0}", ex.Message));
				return;
			}

			// Show device selection
			var ide = Dot42Addin.Ide;
			int minSdkVersion;
			if (!apkFile.Manifest.TryGetMinSdkVersion(out minSdkVersion))
				minSdkVersion = -1;
			var targetFramework = (minSdkVersion > 0) ? Frameworks.Instance.GetBySdkVersion(minSdkVersion) : null;
			var targetVersion = (targetFramework != null) ? targetFramework.Name : null;
			var runner = new ApkRunner(ide, ApkPath, apkFile.Manifest.PackageName, apkFile.Manifest.Activities.Select(x => x.Name).FirstOrDefault(), withDebugging, 0);
			using (var dialog = new DeviceSelectionDialog(ide, IsCompatible, targetVersion)) {
				if (dialog.ShowDialog() == DialogResult.OK) {
					// Run on the device now
					var device = dialog.SelectedDevice;
					ide.LastUsedUniqueId = device.UniqueId;
					
					using (var xDialog = new LauncherDialog(apkPath, withDebugging))
					{
						var stateDialog = xDialog;
						stateDialog.Cancel += (s, x) => runner.Cancel();
						stateDialog.Load += (s, x) => runner.Run(device, stateDialog.SetState);
						stateDialog.ShowDialog();
					}
				}
			}
		}

		/// <summary>
		/// Is the given device compatible with this project?
		/// </summary>
		private bool IsCompatible(DeviceItem item)
		{
			return true; // TODO
		}
		
		/// <summary>
		/// Detect item type.
		/// </summary>
		public override ItemType GetDefaultItemType(string fileName)
		{
			string itemType;
			var frameworkFolder = GetFrameworkFolder();
			if (ItemTypeDetector.TryDetectItemType(fileName, frameworkFolder, out itemType))
				return new ItemType(itemType);
			return base.GetDefaultItemType(fileName);
		}
		
		public override ProjectItem CreateProjectItem(IProjectItemBackendStore item)
		{
			switch (item.ItemType.ItemName) {
				case Dot42Constants.ItemTypeJarReference:
					return new JarReferenceProjectItem(Project, item);
			}
			return base.CreateProjectItem(item);
		}

		/// <summary>
		/// Gets the full path of the APK resulting from the current configuration.
		/// </summary>
		private string ApkPath
		{
			get
			{
				var apkFilename = MSBuildProject.GetEvaluatedProperty("ApkFilename");
				var outputPath = MSBuildProject.GetEvaluatedProperty("OutputPath") ?? "";
				return Path.Combine(Path.Combine(MSBuildProject.Directory, outputPath), apkFilename);
			}
		}

		/// <summary>
		/// Load the framework folder of the current project.
		/// </summary>
		private string GetFrameworkFolder()
		{
			var buildProject = MSBuildProject;
			var folder = (buildProject != null) ? buildProject.GetEvaluatedProperty("TargetFrameworkDirectory") : null;
			return folder ?? string.Empty;
		}

		/// <summary>
		/// Gets the project as MSBuild project
		/// </summary>
		MSBuildBasedProject MSBuildProject {
			get { return (MSBuildBasedProject)Project; }
		}
	}
}
