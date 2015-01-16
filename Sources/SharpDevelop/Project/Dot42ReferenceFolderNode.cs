
using System;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of Dot42ReferenceFolderNode.
	/// </summary>
	public class Dot42ReferenceFolderNode : ReferenceFolder
	{
		public Dot42ReferenceFolderNode(IProject project) : base(project)
		{
			foreach (ProjectItem item in project.Items) {
				if (item is JarReferenceProjectItem) {
					new CustomNode().AddTo(this);
					break;
				}
			}
		}
		
		internal new void ShowReferences() {
			base.ShowReferences();
			ShowJarReferences();
		}
		
		private void ShowJarReferences() {
			foreach (ProjectItem item in Project.Items) {
				if (item is JarReferenceProjectItem) {
					var referenceNode = new JarReferenceNode((JarReferenceProjectItem)item);
					referenceNode.InsertSorted(this);
				}
			}
		}
		
		protected override void Initialize()
		{
			base.Initialize();
			ShowJarReferences();
		}
	}
}
