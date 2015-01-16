
using System;
using System.Windows.Forms;
using Dot42.Ide.Project;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of AddJarReferenceCommand.
	/// </summary>
	public class AddJarReferenceCommand : AbstractCommand
	{
		/// <summary>
		/// Add jar reference
		/// </summary>
		public override void Run()
		{
			var node = Owner as AbstractProjectBrowserTreeNode;
			var project = (node != null) ? node.Project : ProjectService.CurrentProject;
			if (project == null) {
				return;
			}
			LoggingService.Info("Show add jar reference dialog for " + project.FileName);
			using (var dialog = new AddJarReferenceDialog()) {
				if (dialog.ShowDialog(ICSharpCode.SharpDevelop.Gui.WorkbenchSingleton.MainWin32Window) == DialogResult.OK) 
				{
                    var jarPath = dialog.JarPath;
                    var libName = dialog.LibraryName;
                    var importCode = dialog.ImportCode;

                    var item = new JarReferenceProjectItem(project, jarPath);
                    if (!string.IsNullOrEmpty(libName))
                    {
                        item.SetMetadata("LibraryName", libName);
                    }
                    if (importCode)
                    {
                        item.SetMetadata("ImportCode", "yes");
                    }
                    ProjectService.AddProjectItem(project, item);

					project.Save();
					
					ProjectBrowserPad.Instance.ProjectBrowserControl.RefreshView();
				}
			}
		}
	}
}
