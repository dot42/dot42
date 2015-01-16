
using System;
using Dot42.Ide.Editors.Menu;
using Dot42.Ide.Editors.XmlResource;
using Dot42.Ide.Project;
using Dot42.SharpDevelop.Editors.Menu;
using Dot42.SharpDevelop.Editors.XmlResource;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Editors
{
	/// <summary>
	/// Description of XmlEditorDisplayBinding.
	/// </summary>
	public class XmlEditorDisplayBinding : IDisplayBinding
	{
		public bool IsPreferredBindingForFile(string fileName)
		{			
			var itemType = GetItemType(fileName);
			switch (itemType) {
				case Dot42Constants.ItemTypeMenuResource:
				case Dot42Constants.ItemTypeValuesResource:
					return true;
				default:
					return false;
			}
		}
		
		public bool CanCreateContentForFile(string fileName)
		{
			return IsPreferredBindingForFile(fileName);
		}
		
		public double AutoDetectFileContent(string fileName, System.IO.Stream fileContent, string detectedMimeType)
		{
			return 1;
		}
		
		public ICSharpCode.SharpDevelop.Gui.IViewContent CreateContentForFile(OpenedFile file)
		{
			var itemType = GetItemType(file.FileName);
			switch (itemType) {
				case Dot42Constants.ItemTypeMenuResource:
					{
						var ide = Dot42Addin.Ide;
						var serviceProvider = Dot42Addin.ServiceProvider;
						var model = new XmlMenuViewModel(file);
						var control = new XmlMenuDesignerControl(ide, serviceProvider, model);
						return new XmlEditorView(file, model, control);
					}
				case Dot42Constants.ItemTypeValuesResource:
					{
						var model = new XmlResourceViewModel(file);
						var control = new XmlResourceDesignerControl(model);
						return new XmlEditorView(file, model, control);
					}
				default:
					return null;
			}
		}
		
		/// <summary>
		/// Gets the itemtype for the given file.
		/// </summary>
		private static string GetItemType(string fileName) {
			var project = ProjectService.CurrentProject;
			if (project == null)
				return null;
			var item = project.FindFile(fileName);
			if (item == null)
				return null;
			return item.ItemType.ItemName;
		}
	}
}
