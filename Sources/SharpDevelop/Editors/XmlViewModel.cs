
using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using Dot42.Ide.Editors;
using Dot42.Ide.Serialization;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Editor;
using TallComponents.Common.Extensions;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Dot42.SharpDevelop.Editors
{
	/// <summary>
	/// Description of XmlViewModel.
	/// </summary>
	internal abstract class XmlViewModel : IViewModel, IDataErrorInfo, IDocument
	{
		private readonly OpenedFile file;
		private long dirtyTime;

		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		
		protected XmlViewModel(OpenedFile file)
		{
			this.file = file;
		}
		
		/// <summary>
		/// Property indicating if there is a pending change in the underlying text buffer
		/// that needs to be reflected in the ViewModel.
		/// 
		/// Used by DoIdle to determine if we need to sync.
		/// </summary>
		bool BufferDirty
		{
			get;
			set;
		}

		/// <summary>
		/// Property indicating if there is a pending change in the ViewModel that needs to
		/// be committed to the underlying text buffer.
		/// 
		/// Used by DoIdle to determine if we need to sync.
		/// </summary>
		public bool DesignerDirty {
			
			get;set; }
		
		public abstract SerializationNode Root { get; set; }
		
		/// <summary>
		/// Called on idle time. This is when we check if the designer is out of sync with the underlying text buffer.
		/// </summary>
		public void DoIdle()
		{
			if (DesignerDirty) {
				file.IsDirty = true;
			}
		}

		/// <summary>
		/// Load the model from the underlying text buffer.
		/// </summary>
		internal void LoadModelFromXmlModel(OpenedFile file, Stream stream)
		{
			try
			{
				//var document = GetParseTree();
				var serializer = new OpenedFileSerializer(file);
				var document = serializer.Parse(stream);
				LoadModelFromXmlModel(document);
			}
			catch (Exception e)
			{
				//Display error message
				MessageService.ShowHandledException(e, string.Format("Invalid document format: {0}", e.Message));
			}

			BufferDirty = false;

			// Update designer view
			OnViewModelChanged();
		}
		
		/// <summary>
		/// Call NotifyPropertyChanged with the name of the root property.
		/// </summary>
		protected abstract void OnViewModelChanged();

		/// <summary>
		/// Load the model from the underlying text buffer.
		/// </summary>
		protected abstract void LoadModelFromXmlModel(XDocument document);

		/// <summary>
		/// This method is called when it is time to save the designer values to the
		/// underlying buffer.
		/// </summary>
		/// <param name="undoEntry"></param>
		internal void SaveModelToXmlModel(OpenedFile file, Stream stream)
		{
			try
			{
				var documentFromDesignerState = SaveModelToXmlModel();

				// Wrap the buffer sync and the formatting in one undo unit.
				var serializer = new OpenedFileSerializer(file);
				serializer.Save(documentFromDesignerState, stream);
				DesignerDirty = false;
			}
			catch (Exception)
			{
				// if the synchronization fails then we'll just try again in a second.
				dirtyTime = Environment.TickCount;
			}
			finally
			{
			}
		}

		/// <summary>
		/// This method is called when it is time to save the designer values to the underlying buffer.
		/// </summary>
		protected abstract XDocument SaveModelToXmlModel();
		
		void IViewModel.Close()
		{
		}

		protected void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		#region IDataErrorInfo
		
		public string this[string columnName] {
			get {
				return GetDataErrorInfo(columnName);
			}
		}
		
		protected virtual string GetDataErrorInfo(string columnName) {
			return null;
		}
		
		string IDataErrorInfo.Error {
			get {
				throw new NotImplementedException();
			}
		}
		
		#endregion

		#region IDocument
		event EventHandler<TextChangeEventArgs> IDocument.Changing {
			add {
				throw new NotImplementedException();
			}
			remove {
				throw new NotImplementedException();
			}
		}
		
		event EventHandler<TextChangeEventArgs> IDocument.Changed {
			add {
				throw new NotImplementedException();
			}
			remove {
				throw new NotImplementedException();
			}
		}
		
		event EventHandler ITextBuffer.TextChanged {
			add {
				throw new NotImplementedException();
			}
			remove {
				throw new NotImplementedException();
			}
		}
		
		public string Text {
			get {
				var document = SaveModelToXmlModel();
				return document.ToString();
			}
			set {
				var document = XDocument.Parse(value);
				LoadModelFromXmlModel(document);
			}
		}
		
		int IDocument.TotalNumberOfLines {
			get {
				throw new NotImplementedException();
			}
		}
		
		ITextBufferVersion ITextBuffer.Version {
			get {
				throw new NotImplementedException();
			}
		}
		
		int ITextBuffer.TextLength {
			get {
				throw new NotImplementedException();
			}
		}
		
		IDocumentLine IDocument.GetLine(int lineNumber)
		{
			throw new NotImplementedException();
		}
		
		IDocumentLine IDocument.GetLineForOffset(int offset)
		{
			throw new NotImplementedException();
		}
		
		int IDocument.PositionToOffset(int line, int column)
		{
			throw new NotImplementedException();
		}
		
		ICSharpCode.NRefactory.Location IDocument.OffsetToPosition(int offset)
		{
			throw new NotImplementedException();
		}
		
		void IDocument.Insert(int offset, string text)
		{
			throw new NotImplementedException();
		}
		
		void IDocument.Insert(int offset, string text, AnchorMovementType defaultAnchorMovementType)
		{
			throw new NotImplementedException();
		}
		
		void IDocument.Remove(int offset, int length)
		{
			throw new NotImplementedException();
		}
		
		void IDocument.Replace(int offset, int length, string newText)
		{
			throw new NotImplementedException();
		}
		
		void IDocument.StartUndoableAction()
		{
			throw new NotImplementedException();
		}
		
		void IDocument.EndUndoableAction()
		{
			throw new NotImplementedException();
		}
		
		IDisposable IDocument.OpenUndoGroup()
		{
			throw new NotImplementedException();
		}
		
		ITextAnchor IDocument.CreateAnchor(int offset)
		{
			throw new NotImplementedException();
		}
		
		ITextBuffer ITextBuffer.CreateSnapshot()
		{
			throw new NotImplementedException();
		}
		
		ITextBuffer ITextBuffer.CreateSnapshot(int offset, int length)
		{
			throw new NotImplementedException();
		}
		
		TextReader ITextBuffer.CreateReader()
		{
			throw new NotImplementedException();
		}
		
		TextReader ITextBuffer.CreateReader(int offset, int length)
		{
			throw new NotImplementedException();
		}
		
		char ITextBuffer.GetCharAt(int offset)
		{
			throw new NotImplementedException();
		}
		
		string ITextBuffer.GetText(int offset, int length)
		{
			throw new NotImplementedException();
		}
		
		object IServiceProvider.GetService(Type serviceType)
		{
			throw new NotImplementedException();
		}
		#endregion // IDocument
	}
}
