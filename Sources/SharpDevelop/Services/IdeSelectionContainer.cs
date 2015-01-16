using System;
using Dot42.Ide;

namespace Dot42.SharpDevelop.Services
{
	/// <summary>
	/// Description of IdeSelectionContainer.
	/// </summary>
	public class IdeSelectionContainer : IIdeSelectionContainer
	{
		public IdeSelectionContainer(IServiceProvider serviceProvider)
		{
		}
		
		public object SelectedObject {
			set { }
		}
	}
}
