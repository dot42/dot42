using System;
using Dot42.Ide;
using ICSharpCode.SharpDevelop.Gui;

namespace Dot42.SharpDevelop.Services
{
	/// <summary>
	/// Output pane for Dot42
	/// </summary>
	public class IdeOutputPane : IIdeOutputPane
	{
		private MessageViewCategory view;
			
		public void LogLine(string line)
		{
			EnsureLoaded();
			view.AppendLine(line);
		}
		
		public void EnsureLoaded()
		{
			if (view == null) {
				MessageViewCategory.Create(ref view, "Dot42");
			}
		}
	}
}
