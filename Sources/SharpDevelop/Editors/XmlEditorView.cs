
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Dot42.Ide.Editors;
using ICSharpCode.Core;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace Dot42.SharpDevelop.Editors
{
	/// <summary>
	/// Description of XmlEditorView.
	/// </summary>
	public sealed class XmlEditorView : AbstractViewContent, IFileDocumentProvider
	{
		private readonly XmlViewModel viewModel;
		private readonly IDesignerControl control;
		
		/// <summary>
		/// Default ctor
		/// </summary>
		internal XmlEditorView(OpenedFile file, XmlViewModel viewModel, IDesignerControl control)
		{
			this.viewModel = viewModel;
			this.control = control;
			Files.Add(file);
			file.ForceInitializeView(this);		
			ComponentDispatcher.ThreadIdle += OnIdle;
		}
		
		private void OnIdle(object sender, EventArgs e) {
			control.DoIdle();
		}
		
		public override object Control {
			get {
				return control;
			}
		}
		
		/// <summary>
		/// Cleanup
		/// </summary>
		public override void Dispose()
		{
			ComponentDispatcher.ThreadIdle -= OnIdle;
			base.Dispose();
		}
		
		/// <summary>
		/// Saves the content to the location <code>fileName</code>
		/// </summary>
		/// <remarks>
		/// When the user switches between multiple views editing the same file, a view
		/// change will trigger one view content to save that file into a memory stream
		/// and the other view content will load the file from that memory stream.
		/// </remarks>
		public override void Save(OpenedFile file, Stream stream)
		{
			AnalyticsMonitorService.TrackFeature(typeof(XmlEditorView), "Save");
			viewModel.SaveModelToXmlModel(file, stream);
			this.TitleName = Path.GetFileName(file.FileName);
			this.TabPageText = this.TitleName;
		}
		
		/// <summary>
		/// Load or reload the content of the specified file from the stream.
		/// </summary>
		/// <remarks>
		/// When the user switches between multiple views editing the same file, a view
		/// change will trigger one view content to save that file into a memory stream
		/// and the other view content will load the file from that memory stream.
		/// </remarks>
		public override void Load(OpenedFile file, Stream stream)
		{
			AnalyticsMonitorService.TrackFeature(typeof(XmlEditorView), "Load");
			viewModel.LoadModelFromXmlModel(file, stream);
		}
		
		public override bool IsReadOnly {
			get { return false; }
		}

		
		IDocument IFileDocumentProvider.GetDocumentForFile(OpenedFile file)
		{
			return viewModel;
		}
	}
}
