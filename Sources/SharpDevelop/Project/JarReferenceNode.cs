using System;
using System.Diagnostics;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of JarReferenceNode.
	/// </summary>
	public class JarReferenceNode : AbstractProjectBrowserTreeNode
	{
		private readonly JarReferenceProjectItem item;
		
		public JarReferenceNode(JarReferenceProjectItem item)
		{
			this.item = item;
			Tag = item;
			
			ContextmenuAddinTreePath = "/SharpDevelop/Pads/ProjectBrowser/ContextMenu/JarReferenceNode";
			SetIcon("Icons.16x16.Reference");
			Text = item.ShortName;
		}
		
		#region Cut & Paste
		public override bool EnableDelete {
			get {
				return !Project.ReadOnly;
			}
		}
		
		public override void Delete()
		{
			IProject project = Project;
			ProjectService.RemoveProjectItem(item.Project, item);
			Debug.Assert(Parent != null);
			Debug.Assert(Parent is Dot42ReferenceFolderNode);
			((Dot42ReferenceFolderNode)Parent).ShowReferences();
			project.Save();
		}
		#endregion
		
		public override object AcceptVisitor(ProjectBrowserTreeNodeVisitor visitor, object data)
		{
			return visitor.Visit(this, data);
		}
	}
}
