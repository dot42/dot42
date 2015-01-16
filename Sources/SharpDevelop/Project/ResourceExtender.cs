
using System;
using System.IO;
using Dot42.Ide.Extenders;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of ResourceExtender.
	/// </summary>
	public class ResourceExtender : ResourceExtenderBase
	{
		private readonly FileProjectItem projectItem;
		
		/// <summary>
		/// Default ctor
		/// </summary>
		public ResourceExtender(FileProjectItem projectItem)
		{
			this.projectItem = projectItem;
		}
		
		/// <summary>
		/// Gets/sets the filename of the project item
		/// </summary>
		protected override string FileName {
			get {
				return projectItem.FileName;
			}
			set {
				var newPath = Path.Combine(Path.GetDirectoryName(projectItem.FileName), value);
				RenameFile(projectItem.Project, projectItem.FileName, newPath);
			}
		}
		
				/// <summary>
		/// Renames file as well as files it is dependent upon.
		/// </summary>
		public static void RenameFile(IProject p, string oldFileName, string newFileName)
		{
			FileService.RenameFile(oldFileName, newFileName, false);
			if (p != null) {
				string oldPrefix = Path.GetFileNameWithoutExtension(oldFileName) + ".";
				string newPrefix = Path.GetFileNameWithoutExtension(newFileName) + ".";
				foreach (ProjectItem item in p.Items) {
					FileProjectItem fileItem = item as FileProjectItem;
					if (fileItem == null)
						continue;
					string dependentUpon = fileItem.DependentUpon;
					if (string.IsNullOrEmpty(dependentUpon))
						continue;
					string directory = Path.GetDirectoryName(fileItem.FileName);
					dependentUpon = Path.Combine(directory, dependentUpon);
					if (FileUtility.IsEqualFileName(dependentUpon, oldFileName)) {
						fileItem.DependentUpon = FileUtility.GetRelativePath(directory, newFileName);
						string fileName = Path.GetFileName(fileItem.FileName);
						if (fileName.StartsWith(oldPrefix)) {
							RenameFile(p, fileItem.FileName, Path.Combine(directory, newPrefix + fileName.Substring(oldPrefix.Length)));
						}
					}
				}
			}
		}
	}
}
