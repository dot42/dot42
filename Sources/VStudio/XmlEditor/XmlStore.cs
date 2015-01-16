using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.XmlEditor;

namespace Dot42.VStudio.XmlEditor
{
    internal class XmlStore : IXmlStore
    {
        private readonly Microsoft.VisualStudio.XmlEditor.XmlStore store;

        public event EventHandler UndoRedoCompleted;
        public event EventHandler<EditingScopeEventArgs> EditingScopeCompleted;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal XmlStore(Microsoft.VisualStudio.XmlEditor.XmlStore store)
        {
            this.store = store;
            store.UndoRedoCompleted += StoreOnUndoRedoCompleted;
            store.EditingScopeCompleted += OnStoreEditingScopeCompleted;
        }

        void OnStoreEditingScopeCompleted(object sender, XmlEditingScopeEventArgs e)
        {
            if (EditingScopeCompleted != null)
            {
                EditingScopeCompleted(sender, new EditingScopeEventArgs(e.EditingScope.UserState));
            }
        }

        /// <summary>
        /// Disconnect event handlers
        /// </summary>
        void IXmlStore.Disconnect()
        {
            store.UndoRedoCompleted -= StoreOnUndoRedoCompleted;
            store.EditingScopeCompleted -= OnStoreEditingScopeCompleted;
        }

        object IXmlStore.UndoManager
        {
            get { return store.UndoManager; }
            set { store.UndoManager = (IOleUndoManager) value; }
        }

        IXmlModel IXmlStore.OpenXmlModel(Uri uri)
        {
            var model =store.OpenXmlModel(uri);
            if (model == null)
                return null;
            return new XmlModel(model);
        }

        private void StoreOnUndoRedoCompleted(object sender, XmlEditingScopeEventArgs xmlEditingScopeEventArgs)
        {
            if (UndoRedoCompleted != null)
            {
                UndoRedoCompleted(this, EventArgs.Empty);
            }
        }

        void IDisposable.Dispose()
        {
            store.Dispose();
        }
    }
}
