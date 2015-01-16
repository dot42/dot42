
using System;
using System.IO;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of JarReferenceProjectItem.
	/// </summary>
	public class JarReferenceProjectItem : ProjectItem
	{
		public JarReferenceProjectItem(IProject project, string include) :
			base(project, new ItemType("JarReference"))
		{
			Include = include;
		}
		
		internal JarReferenceProjectItem(IProject project, IProjectItemBackendStore buildItem) :
			base(project, buildItem)
		{
		}
		
		public string ShortName {
			get{ return Path.GetFileName(FileName); }
		}
	}
}
