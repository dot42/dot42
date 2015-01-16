
using System;
using Dot42.Ide.Project;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of Dot42ProjectNodeBuilder.
	/// </summary>
	public class Dot42ProjectNodeBuilder : IProjectNodeBuilder
	{
		public bool CanBuildProjectTree(IProject project)
		{
			var prj = project as MSBuildBasedProject;
			return (prj != null) && prj.HasProjectType(new Guid(Dot42Constants.Dot42ProjectType));
		}
		
		public System.Windows.Forms.TreeNode AddProjectNode(System.Windows.Forms.TreeNode motherNode, IProject project)
		{
			var prjNode = new Dot42ProjectNode(project);
			prjNode.InsertSorted(motherNode);
			new Dot42ReferenceFolderNode(project).AddTo(prjNode);
			return prjNode;
		}
	}
}
