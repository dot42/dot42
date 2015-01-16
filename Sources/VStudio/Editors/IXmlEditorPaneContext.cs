using System;
using Dot42.Ide;
using Dot42.Ide.Editors;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Dot42.VStudio.Editors
{
    /// <summary>
    /// Helper to instantiate a property editor pane.
    /// </summary>
    internal interface IXmlEditorPaneContext
    {
        /// <summary>
        /// Create the appropriate designer control.
        /// </summary>
        IDesignerControl CreateDesigner(IIde ide, IXmlStore store, IXmlModel model, IServiceProvider serviceProvider, IVsTextLines textBuffer);

        /// <summary>
        /// Is the given MSBuild item type supported by this editor?
        /// </summary>
        bool SupportsItemType(string itemType);
    }
}
