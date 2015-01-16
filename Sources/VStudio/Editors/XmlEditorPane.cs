using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Dot42.Ide.Editors;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;

namespace Dot42.VStudio.Editors
{
    /// <summary>
    /// This control hosts the editor and is responsible for handling the commands targeted to the editor 
    /// </summary>
    [ComVisible(true)]
    internal sealed class XmlEditorPane : WindowPane, IOleComponent, IVsDeferredDocView, IVsLinkedUndoClient
    {
        private readonly Dot42Package thisPackage;
        private readonly IXmlEditorPaneContext context;
        private string _fileName = string.Empty;
        private IDesignerControl designerControl;
        private readonly IVsTextLines textBuffer;
        private uint componentId;
        private IOleUndoManager undoManager;
        private IXmlStore store;
        private IXmlModel model;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal XmlEditorPane(Dot42Package package, IXmlEditorPaneContext context, string fileName, IVsTextLines textBuffer)
            : base(null)
        {
            thisPackage = package;
            this.context = context;
            _fileName = fileName;
            this.textBuffer = textBuffer;
        }

        #region "Window.Pane Overrides"
        protected override void OnClose()
        {
            // unhook from Undo related services
            if (undoManager != null)
            {
                var linkCapableUndoMgr = (IVsLinkCapableUndoManager)undoManager;
                if (linkCapableUndoMgr != null)
                {
                    linkCapableUndoMgr.UnadviseLinkedUndoClient();
                }

                // Throw away the undo stack etc.
                // It is important to “zombify” the undo manager when the owning object is shutting down.
                // This is done by calling IVsLifetimeControlledObject.SeverReferencesToOwner on the undoManager.
                // This call will clear the undo and redo stacks. This is particularly important to do if
                // your undo units hold references back to your object. It is also important if you use
                // "mdtStrict" linked undo transactions as this sample does (see IVsLinkedUndoTransactionManager). 
                // When one object involved in linked undo transactions clears its undo/redo stacks, then 
                // the stacks of the other documents involved in the linked transaction will also be cleared. 
                var lco = (IVsLifetimeControlledObject)undoManager;
                lco.SeverReferencesToOwner();
                undoManager = null;
            }

            var mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            mgr.FRevokeComponent(componentId);

            this.Dispose(true);

            base.OnClose();
        }
        #endregion

        /// <summary>
        /// Called after the WindowPane has been sited with an IServiceProvider from the environment
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Create and initialize the editor
            #region Register with IOleComponentManager
            var componentManager = (IOleComponentManager)GetService(typeof(SOleComponentManager));
            if (this.componentId == 0 && componentManager != null)
            {
                var crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 100;
                int hr = componentManager.FRegisterComponent(this, crinfo, out this.componentId);
                ErrorHandler.Succeeded(hr);
            }
            #endregion

            var resources = new ComponentResourceManager(typeof(XmlEditorPane));

            #region Hook Undo Manager
            // Attach an IOleUndoManager to our WindowFrame. Merely calling QueryService 
            // for the IOleUndoManager on the site of our IVsWindowPane causes an IOleUndoManager
            // to be created and attached to the IVsWindowFrame. The WindowFrame automaticall 
            // manages to route the undo related commands to the IOleUndoManager object.
            // Thus, our only responsibilty after this point is to add IOleUndoUnits to the 
            // IOleUndoManager (aka undo stack).
            undoManager = (IOleUndoManager)GetService(typeof(SOleUndoManager));

            // In order to use the IVsLinkedUndoTransactionManager, it is required that you
            // advise for IVsLinkedUndoClient notifications. This gives you a callback at 
            // a point when there are intervening undos that are blocking a linked undo.
            // You are expected to activate your document window that has the intervening undos.
            if (undoManager != null)
            {
                IVsLinkCapableUndoManager linkCapableUndoMgr = (IVsLinkCapableUndoManager)undoManager;
                if (linkCapableUndoMgr != null)
                {
                    linkCapableUndoMgr.AdviseLinkedUndoClient(this);
                }
            }
            #endregion

            // hook up our 
            var dteVersion = Dot42Package.DteVersion;
            var xmlEditorService = XmlEditorServiceProvider.GetEditorService(this, dteVersion);
            if (xmlEditorService == null)
                throw new InvalidOperationException("XmlEditorService required");
            store = xmlEditorService.CreateXmlStore();
            store.UndoManager = undoManager;

            model = store.OpenXmlModel(new Uri(_fileName));

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            //designerControl = new VsDesignerControl(new ViewModel(store, model, this, textBuffer));
            designerControl = context.CreateDesigner(thisPackage.Ide, store, model, this, textBuffer);
            base.Content = designerControl;

            RegisterIndependentView(true);

            var mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (null != mcs)
            {
                // Now create one object derived from MenuCommnad for each command defined in
                // the CTC file and add it to the command service.

                // For each command we have to define its id that is a unique Guid/integer pair, then
                // create the OleMenuCommand object for this command. The EventHandler object is the
                // function that will be called when the user will select the command. Then we add the 
                // OleMenuCommand to the menu service.  The addCommand helper function does all this for us.
                AddCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.NewWindow, OnNewWindow, OnQueryNewWindow);
                AddCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.ViewCode, OnViewCode, OnQueryViewCode);
            }
        }

        /// <summary>
        /// returns the name of the file currently loaded
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RegisterIndependentView(false);

                using (model)
                {
                    model = null;
                }
                using (store)
                {
                    store = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (_fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            var runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            int hr = runningDocTable.FindAndLockDocument(
                (uint)_VSRDTFLAGS.RDT_ReadLock,
                _fileName,
                out hierarchy,
                out itemID,
                out docData,
                out docCookie
            );
            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

            // Check ff the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }

        /// <summary>
        /// Helper function used to add commands using IMenuCommandService
        /// </summary>
        /// <param name="mcs"> The IMenuCommandService interface.</param>
        /// <param name="menuGroup"> This guid represents the menu group of the command.</param>
        /// <param name="cmdID"> The command ID of the command.</param>
        /// <param name="commandEvent"> An EventHandler which will be called whenever the command is invoked.</param>
        /// <param name="queryEvent"> An EventHandler which will be called whenever we want to query the status of
        /// the command.  If null is passed in here then no EventHandler will be added.</param>
        private static void AddCommand(IMenuCommandService mcs, Guid menuGroup, int cmdID, EventHandler commandEvent, EventHandler queryEvent)
        {
            // Create the OleMenuCommand from the menu group, command ID, and command event
            var menuCommandID = new CommandID(menuGroup, cmdID);
            var command = new OleMenuCommand(commandEvent, menuCommandID);

            // Add an event handler to BeforeQueryStatus if one was passed in
            if (null != queryEvent)
            {
                command.BeforeQueryStatus += queryEvent;
            }

            // Add the command using our IMenuCommandService instance
            mcs.AddCommand(command);
        }

        /// <summary>
        /// Registers an independent view with the IVsTextManager so that it knows
        /// the user is working with a view over the text buffer. This will trigger
        /// the text buffer to prompt the user whether to reload the file if it is
        /// edited outside of the environment.
        /// </summary>
        /// <param name="subscribe">True to subscribe, false to unsubscribe</param>
        void RegisterIndependentView(bool subscribe)
        {
            var textManager = (IVsTextManager)GetService(typeof(SVsTextManager));
            if (textManager == null) 
                return;

            if (subscribe)
            {
                textManager.RegisterIndependentView(this, textBuffer);
            }
            else
            {
                textManager.UnregisterIndependentView(this, textBuffer);
            }
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            string resourceValue;
            var resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            var packageGuid = thisPackage.GetType().GUID;
            var hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

        #region Commands

        private void OnQueryNewWindow(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            command.Enabled = true;
        }

        private void OnNewWindow(object sender, EventArgs e)
        {
            NewWindow();
        }

        private void OnQueryViewCode(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;
            command.Enabled = true;
        }

        private void OnViewCode(object sender, EventArgs e)
        {
            ViewCode();
        }

        private void NewWindow()
        {
            int hr = VSConstants.S_OK;

            var uishellOpenDocument = (IVsUIShellOpenDocument)GetService(typeof(SVsUIShellOpenDocument));
            if (uishellOpenDocument != null)
            {
                var windowFrameOrig = (IVsWindowFrame)GetService(typeof(SVsWindowFrame));
                if (windowFrameOrig != null)
                {
                    IVsWindowFrame windowFrameNew;
                    var LOGVIEWID_Primary = Guid.Empty;
                    hr = uishellOpenDocument.OpenCopyOfStandardEditor(windowFrameOrig, ref LOGVIEWID_Primary, out windowFrameNew);
                    if (windowFrameNew != null)
                        hr = windowFrameNew.Show();
                    ErrorHandler.ThrowOnFailure(hr);
                }
            }
        }

        private void ViewCode()
        {
            var XmlTextEditorGuid = new Guid("FA3CD31E-987B-443A-9B81-186104E8DAC1");

            // Open the referenced document using our editor.
            IVsWindowFrame frame;
            IVsUIHierarchy hierarchy;
            uint itemid;
            VsShellUtilities.OpenDocumentWithSpecificEditor(this, model.Name,
                XmlTextEditorGuid, VSConstants.LOGVIEWID_Primary, out hierarchy, out itemid, out frame);
            ErrorHandler.ThrowOnFailure(frame.Show());
        }

        #endregion

        #region IVsLinkedUndoClient

        public int OnInterveningUnitBlockingLinkedUndo()
        {
            return VSConstants.E_FAIL;
        }

        #endregion

        #region IVsDeferredDocView

        /// <summary>
        /// Assigns out parameter with the Guid of the EditorFactory.
        /// </summary>
        /// <param name="pGuidCmdId">The output parameter that receives a value of the Guid of the EditorFactory.</param>
        /// <returns>S_OK if Marshal operations completed successfully.</returns>
        int IVsDeferredDocView.get_CmdUIGuid(out Guid pGuidCmdId)
        {
            pGuidCmdId = GuidList.Guids.guidXmlEditorFactory;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Assigns out parameter with the document view being implemented.
        /// </summary>
        /// <param name="ppUnkDocView">The parameter that receives a reference to current view.</param>
        /// <returns>S_OK if Marshal operations completed successfully.</returns>
        [EnvironmentPermission(SecurityAction.Demand)]
        int IVsDeferredDocView.get_DocView(out IntPtr ppUnkDocView)
        {
            ppUnkDocView = Marshal.GetIUnknownForObject(this);
            return VSConstants.S_OK;
        }

        #endregion

        #region IOleComponent

        int IOleComponent.FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return VSConstants.S_OK;
        }

        int IOleComponent.FDoIdle(uint grfidlef)
        {
            if (designerControl != null)
            {
                designerControl.DoIdle();
            }
            return VSConstants.S_OK;
        }

        int IOleComponent.FPreTranslateMessage(MSG[] pMsg)
        {
            return VSConstants.S_OK;
        }

        int IOleComponent.FQueryTerminate(int fPromptUser)
        {
            return 1; //true
        }

        int IOleComponent.FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return VSConstants.S_OK;
        }

        IntPtr IOleComponent.HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        void IOleComponent.OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) { }
        void IOleComponent.OnAppActivate(int fActive, uint dwOtherThreadID) { }
        void IOleComponent.OnEnterState(uint uStateID, int fEnter) { }
        void IOleComponent.OnLoseActivation() { }
        void IOleComponent.Terminate() { }

        #endregion
    }
}
