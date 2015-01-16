using System;

namespace Dot42.VStudio.XmlEditor
{
    public interface IXmlStore : IDisposable
    {
        event EventHandler UndoRedoCompleted;
        event EventHandler<EditingScopeEventArgs> EditingScopeCompleted;

        /// <summary>
        /// Disconnect event handlers
        /// </summary>
        void Disconnect();

        object UndoManager { get; set; }

        IXmlModel OpenXmlModel(Uri uri);
    }
}
