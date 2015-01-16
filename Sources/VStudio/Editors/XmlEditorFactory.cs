using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dot42.VStudio.Editors.Menu;
using Dot42.VStudio.Editors.XmlResource;
using Dot42.VStudio.Flavors;
using Dot42.VStudio.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Dot42.VStudio.Editors
{
    /// <summary>
    /// Editor factory for all Dot42 XML formats
    /// </summary>
    [Guid(GuidList.Strings.guidXmlEditorFactory)]
    internal class XmlEditorFactory : IVsEditorFactory, IDisposable
    {
        private readonly Dot42Package package;
        private ServiceProvider serviceProvider;

        private readonly IXmlEditorPaneContext[] editorContexts;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlEditorFactory(Dot42Package package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            this.package = package;
            editorContexts = new IXmlEditorPaneContext[] {
                new XmlResourceEditorPaneContext(), 
                new XmlMenuEditorPaneContext(), 
            };
        }

        /// <summary>
        /// Sets the site of the FrameXML Editor Factory
        /// </summary>
        /// <param name="oleServiceProvider">The OLE service provider.</param>
        public int SetSite(IServiceProvider oleServiceProvider)
        {
            this.serviceProvider = new ServiceProvider(oleServiceProvider);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets a service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>An instance of the <typeparamref name="TService"/> class.</returns>
        protected TService GetService<TService>() where TService : class
        {
            if (serviceProvider != null)
                return (TService)serviceProvider.GetService(typeof(TService));

            return null;
        }

        /// <summary>
        /// Maps a logical view to a physical view.
        /// </summary>
        /// <param name="logicalView">The <see cref="Guid"/> of the logical view.</param>
        /// <param name="physicalView">The name of the physical viwe.</param>
        public int MapLogicalView(ref Guid logicalView, out string physicalView)
        {
            physicalView = null;

            bool isSupportedView = false;

            if (VSConstants.LOGVIEWID_Primary == logicalView ||
                VSConstants.LOGVIEWID_Designer == logicalView)
            {
                physicalView = "Design";
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_TextView == logicalView ||
                     VSConstants.LOGVIEWID_Code == logicalView)
            {
                isSupportedView = true;
            }

            return isSupportedView ? VSConstants.S_OK : VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Creates an instance of the FrameXML editor.
        /// </summary>
        public int CreateEditorInstance(
               uint createEditorFlags,
               string documentMoniker,
               string physicalView,
               IVsHierarchy hierarchy,
               uint itemid,
               IntPtr docDataExisting,
               out IntPtr docView,
               out IntPtr docData,
               out string editorCaption,
               out Guid commandUIGuid,
               out int createDocumentWindowFlags)
        {
            // Initialize out parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = new Guid(GuidList.Strings.guidXmlEditorFactory);
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
                return VSConstants.E_INVALIDARG;

            // Get item type
            var projectItem = hierarchy.GetProjectItemFromHierarchy(itemid);
            var itemType = projectItem.GetProjectItemType(serviceProvider);
            IXmlEditorPaneContext context = null;
            foreach (var ctx in editorContexts)
            {
                if (ctx.SupportsItemType(itemType))
                {
                    context = ctx;
                    break;
                }
            }
            if (context == null)
                return VSConstants.VS_E_UNSUPPORTEDFORMAT;

            // Get the text buffer
            IVsTextLines textLines = GetTextBuffer(docDataExisting, documentMoniker);

            // Assign docData IntPtr to either existing docData or the new text buffer
            docData = docDataExisting != IntPtr.Zero ? docDataExisting : Marshal.GetIUnknownForObject(textLines);

            try
            {
                docView = CreateDocumentView(physicalView, itemid, textLines, out editorCaption, out commandUIGuid, documentMoniker, context);
            }
            finally
            {
                // If we were not able to create a view
                if (docView == IntPtr.Zero)
                {
                    // And we've created a new docData that is not the same as the docDataExisting
                    if (docDataExisting != docData && docData != IntPtr.Zero)
                    {
                        // Release the docData we have created and null it out
                        Marshal.Release(docData);
                        docData = IntPtr.Zero;
                    }
                }
            }

            return VSConstants.S_OK;
        }

		/// <summary>
		/// Closes this instance.
		/// </summary>
		/// <returns></returns>
        public int Close()
        {
            return VSConstants.S_OK;
        }

        private IVsTextLines GetTextBuffer(IntPtr docDataExisting, string documentMoniker)
        {
            IVsTextLines textLines;

            if (docDataExisting == IntPtr.Zero)
            {
                // Create a new IVsTextLines buffer
                textLines = package.CreateInstance<IVsTextLines, VsTextBufferClass>();

                // Set the buffer's site
                ((IObjectWithSite)textLines).SetSite(serviceProvider.GetService(typeof(IServiceProvider)));

                // Explicitly load the data through IVsPersistDocData
                ((IVsPersistDocData)textLines).LoadDocData(documentMoniker);
            }
            else
            {
                // Use the existing text buffer
                var dataObject = Marshal.GetObjectForIUnknown(docDataExisting);

                textLines = dataObject as IVsTextLines;

                if (textLines == null)
                {
                    // Try to get the text buffer from textbuffer provider
                    var textBufferProvider = dataObject as IVsTextBufferProvider;

                    if (textBufferProvider != null)
                        textBufferProvider.GetTextBuffer(out textLines);
                }

                if (textLines == null)
                {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    ErrorHandler.ThrowOnFailure(VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }
            }

            return textLines;
        }

		/// <summary>
		/// Creates the document view.
		/// </summary>
		/// <param name="physicalView">The physical view.</param>
		/// <param name="itemid">The itemid.</param>
		/// <param name="textLines">The text lines.</param>
		/// <param name="editorCaption">The editor caption.</param>
		/// <param name="cmdUI">The CMD UI.</param>
		/// <param name="documentPath">The document path.</param>
		/// <returns></returns>
        private IntPtr CreateDocumentView(string physicalView, uint itemid, IVsTextLines textLines, out string editorCaption, out Guid cmdUI, string documentPath, IXmlEditorPaneContext context)
        {
            // Initialize out parameters
            editorCaption = String.Empty;
            cmdUI = Guid.Empty;

            if (String.IsNullOrEmpty(physicalView))
            {
                // Create Code view as default physical view
                return this.CreateCodeView(textLines, ref editorCaption, ref cmdUI);
            }

            if (physicalView == "Design")
            {
                try
                {
                    // Create Designer View
                    return this.CreateDesignerView(itemid, textLines, ref editorCaption, ref cmdUI, documentPath, context);
                }
                catch (InvalidOperationException ex)
                {
                    var openCodeEditor = MessageBox.Show(ex.Message, null, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
                    if (openCodeEditor)
                    {
                        // Create Code view instead
                        return this.CreateCodeView(textLines, ref editorCaption, ref cmdUI);
                    }

                    // Could not handle exception, rethrow
                    throw;
                }
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            ErrorHandler.ThrowOnFailure(VSConstants.VS_E_UNSUPPORTEDFORMAT);

            return IntPtr.Zero;
        }

		/// <summary>
		/// Creates the designer view.
		/// </summary>
		/// <param name="itemid">The itemid.</param>
		/// <param name="textLines">The text lines.</param>
		/// <param name="editorCaption">The editor caption.</param>
		/// <param name="cmdUI">The CMD UI.</param>
		/// <param name="documentMoniker">The document moniker.</param>
		/// <returns></returns>
        private IntPtr CreateDesignerView(uint itemid, IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI, string documentMoniker, IXmlEditorPaneContext context)
        {
            // Create the editor pane
		    var editorPane = new XmlEditorPane(package, context, documentMoniker, textLines);

            // Set the Command guid and the editor caption
            cmdUI = GuidList.Guids.guidXmlEditorFactory;
            editorCaption = " [Design]";

            return Marshal.GetIUnknownForObject(editorPane);
        }

		/// <summary>
		/// Creates the code view.
		/// </summary>
		/// <param name="textLines">The text lines.</param>
		/// <param name="editorCaption">The editor caption.</param>
		/// <param name="cmdUI">The CMD UI.</param>
		/// <returns></returns>
        private IntPtr CreateCodeView(IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI)
        {
            var window = package.CreateInstance<IVsCodeWindow, VsCodeWindowClass>();

            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            cmdUI = VSConstants.GUID_TextEditorFactory;

            return Marshal.GetIUnknownForObject(window);
        }

    	#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
    	public void Dispose()
    	{
			Dispose(true);
			GC.SuppressFinalize(this);
    	}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{

		}

    	#endregion
    }
}
