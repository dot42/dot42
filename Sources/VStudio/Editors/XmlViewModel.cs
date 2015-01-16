using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Dot42.Ide.Editors;
using Dot42.Ide.Serialization;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Editors
{
    /// <summary>
    /// Base class for XML base view models
    /// </summary>
    internal abstract class XmlViewModel : IViewModel, IDataErrorInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IXmlModel xmlModel;
        private readonly IXmlStore xmlStore;

        private readonly IServiceProvider serviceProvider;
        private readonly IVsTextLines buffer;
        private readonly TextBufferSerializer textBufferSerializer;

        private bool synchronizing;
        private long dirtyTime;
        private readonly EventHandler<EditingScopeEventArgs> editingScopeCompletedHandler;
        private readonly EventHandler undoRedoCompletedHandler;
        private readonly EventHandler bufferReloadedHandler;

        private IXmlLanguageService xmlLanguageService;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XmlViewModel(IXmlStore xmlStore, IXmlModel xmlModel, IServiceProvider provider, IVsTextLines buffer)
        {
            if (xmlModel == null)
                throw new ArgumentNullException("xmlModel");
            if (xmlStore == null)
                throw new ArgumentNullException("xmlStore");
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            BufferDirty = false;
            DesignerDirty = false;

            serviceProvider = provider;            
            this.buffer = buffer;
            textBufferSerializer = new TextBufferSerializer(buffer);

            this.xmlStore = xmlStore;
            // OnUnderlyingEditCompleted
            editingScopeCompletedHandler = OnUnderlyingEditCompleted;
            this.xmlStore.EditingScopeCompleted += editingScopeCompletedHandler;
            // OnUndoRedoCompleted
            undoRedoCompletedHandler = OnUndoRedoCompleted;
            this.xmlStore.UndoRedoCompleted += undoRedoCompletedHandler;

            this.xmlModel = xmlModel;
            // BufferReloaded
            bufferReloadedHandler += BufferReloaded;
            this.xmlModel.BufferReloaded += bufferReloadedHandler;
        }

        /// <summary>
        /// Make sure the model is loaded.
        /// </summary>
        public void Initialize()
        {
            LoadModelFromXmlModel();            
        }

        public void Close()
        {
            //Unhook the events from the underlying XmlStore/XmlModel
            if (xmlStore != null)
            {
                xmlStore.Disconnect();
            }
            if (this.xmlModel != null)
            {
                this.xmlModel.BufferReloaded -= bufferReloadedHandler;
            }
        }

        /// <summary>
        /// Gets the root element
        /// </summary>
        public abstract SerializationNode Root { get; set; }

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
        public bool DesignerDirty
        {
            get;
            set;
        }

        /// <summary>
        /// We must not try and update the XDocument while the XML Editor is parsing as this may cause
        /// a deadlock in the XML Editor!
        /// </summary>
        private bool IsXmlEditorParsing
        {
            get
            {
                var langsvc = GetXmlLanguageService();
                return (langsvc != null) && langsvc.IsParsing;
            }
        }

        /// <summary>
        /// Called on idle time. This is when we check if the designer is out of sync with the underlying text buffer.
        /// </summary>
        public void DoIdle()
        {
            if (BufferDirty || DesignerDirty)
            {
                const int delay = 100;

                if ((Environment.TickCount - dirtyTime) > delay)
                {
                    // Must not try and sync while XML editor is parsing otherwise we just confuse matters.
                    if (IsXmlEditorParsing)
                    {
                        dirtyTime = Environment.TickCount;
                        return;
                    }

                    //If there is contention, give the preference to the designer.
                    if (DesignerDirty)
                    {
                        SaveModelToXmlModel(Resources.SynchronizeBuffer);
                        // We don't do any merging, so just overwrite whatever was in the buffer.
                        BufferDirty = false;
                    }
                    else if (BufferDirty)
                    {
                        LoadModelFromXmlModel();
                    }
                }
            }
        }

        /// <summary>
        /// Load the model from the underlying text buffer.
        /// </summary>
        private void LoadModelFromXmlModel()
        {
            try
            {
                //var document = GetParseTree();
                var document = textBufferSerializer.Parse();
                LoadModelFromXmlModel(document);
            }
            catch (Exception e)
            {
                //Display error message
                ErrorHandler.ThrowOnFailure(VsShellUtilities.ShowMessageBox(serviceProvider,
                    string.Format("Invalid document format: {0}", e.Message),
                    Resources.ProductName,
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST));
            }

            BufferDirty = false;

            // Update designer view
            ViewModelChanged.Fire(this);
        }

        /// <summary>
        /// Load the model from the underlying text buffer.
        /// </summary>
        protected abstract void LoadModelFromXmlModel(XDocument document);

        /// <summary>
        /// This method is called when it is time to save the designer values to the
        /// underlying buffer.
        /// </summary>
        /// <param name="undoEntry"></param>
        void SaveModelToXmlModel(string undoEntry)
        {
            var langsvc = GetXmlLanguageService();

            try
            {
                //We can't edit this file (perhaps the user cancelled a SCC prompt, etc...)
                if (!CanEditFile())
                {
                    DesignerDirty = false;
                    BufferDirty = true;
                    throw new Exception();
                }

                //PopulateModelFromReferencesBindingList();
                //PopulateModelFromContentBindingList();

                var documentFromDesignerState = SaveModelToXmlModel();

                synchronizing = true;

                //var document = GetParseTree();
                var src = (langsvc != null) ? langsvc.GetSource(buffer) : null;
                if (src == null)
                {
                    return;
                }

                langsvc.IsParsing = true; // lock out the background parse thread.

                // Wrap the buffer sync and the formatting in one undo unit.
                src.Save(() => textBufferSerializer.Save(documentFromDesignerState));
                DesignerDirty = false;
            }
            catch (Exception)
            {
                // if the synchronization fails then we'll just try again in a second.
                dirtyTime = Environment.TickCount;
            }
            finally
            {
                if (langsvc != null)
                    langsvc.IsParsing = false;
                synchronizing = false;
            }
        }

        /// <summary>
        /// This method is called when it is time to save the designer values to the underlying buffer.
        /// </summary>
        protected abstract XDocument SaveModelToXmlModel();

        /// <summary>
        /// Fired when all controls should be re-bound.
        /// </summary>
        public event EventHandler ViewModelChanged;

        private void BufferReloaded(object sender, EventArgs e)
        {
            if (!synchronizing)
            {
                BufferDirty = true;
                dirtyTime = Environment.TickCount;
            }
        }

        /// <summary>
        /// Handle undo/redo completion event.  This happens when the user invokes Undo/Redo on a buffer edit operation.
        /// We need to resync when this happens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnUndoRedoCompleted(object sender, EventArgs e)
        {
            if (!synchronizing)
            {
                BufferDirty = true;
                dirtyTime = Environment.TickCount;
            }
        }

        /// <summary>
        /// Handle edit scope completion event.  This happens when the XML editor buffer decides to update
        /// it's XDocument parse tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnUnderlyingEditCompleted(object sender, EditingScopeEventArgs e)
        {
            if (e.EditingScopeUserState != this && !synchronizing)
            {
                BufferDirty = true;
                dirtyTime = Environment.TickCount;
            }
        }

        #region Source Control

        bool? _canEditFile;
        bool _gettingCheckoutStatus;

        /// <summary>
        /// This function asks the QueryEditQuerySave service if it is possible to edit the file.
        /// This can result in an automatic checkout of the file and may even prompt the user for
        /// permission to checkout the file.  If the user says no or the file cannot be edited 
        /// this returns false.
        /// </summary>
        private bool CanEditFile()
        {
            // Cache the value so we don't keep asking the user over and over.
            if (_canEditFile.HasValue)
            {
                return (bool)_canEditFile;
            }

            // Check the status of the recursion guard
            if (_gettingCheckoutStatus)
                return false;

            _canEditFile = false; // assume the worst
            try
            {
                // Set the recursion guard
                _gettingCheckoutStatus = true;

                // Get the QueryEditQuerySave service
                IVsQueryEditQuerySave2 queryEditQuerySave = serviceProvider.GetService(typeof(SVsQueryEditQuerySave)) as IVsQueryEditQuerySave2;

                string filename = xmlModel.Name;

                // Now call the QueryEdit method to find the edit status of this file
                string[] documents = { filename };
                uint result;
                uint outFlags;

                // Note that this function can popup a dialog to ask the user to checkout the file.
                // When this dialog is visible, it is possible to receive other request to change
                // the file and this is the reason for the recursion guard
                int hr = queryEditQuerySave.QueryEditFiles(
                    0,              // Flags
                    1,              // Number of elements in the array
                    documents,      // Files to edit
                    null,           // Input flags
                    null,           // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                    out result,     // result of the checkout
                    out outFlags    // Additional flags
                );
                if (ErrorHandler.Succeeded(hr) && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                {
                    // In this case (and only in this case) we can return true from this function
                    _canEditFile = true;
                }
            }
            finally
            {
                _gettingCheckoutStatus = false;
            }
            return (bool)_canEditFile;
        }

        #endregion

        #region IDataErrorInfo
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get error info for the given column name
        /// </summary>
        string IDataErrorInfo.this[string columnName]
        {
            get { return GetDataErrorInfo(columnName); }
        }

        /// <summary>
        /// Get error info for the given column name
        /// </summary>
        protected virtual string GetDataErrorInfo(string columnName)
        {
            return null;
        }

        /*private string ValidateId()
        {
            if (string.IsNullOrEmpty(this.TemplateID))
            {
                return string.Format(Resources.ValidationRequiredField, Resources.FieldNameId);
            }

            if (this.TemplateID.Length > MaxIdLength)
            {
                return string.Format(Resources.ValidationFieldMaxLength, Resources.FieldNameId, MaxIdLength);
            }
            return null;
        }*/

        #endregion

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Gets a single instance of the Xml language service.
        /// </summary>
        private IXmlLanguageService GetXmlLanguageService()
        {
            if (xmlLanguageService == null)
            {
                xmlLanguageService = XmlEditorServiceProvider.GetEditorService(serviceProvider, Dot42Package.DteVersion).GetLanguageService();
            }
            return xmlLanguageService;
        }
    }    
}
