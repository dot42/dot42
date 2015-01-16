using System;
using System.ComponentModel.Composition;
using Dot42.Ide;
using Dot42.Ide.Editors;
using Dot42.Ide.Editors.Menu;
using Dot42.Ide.Project;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio.TextManager.Interop;
using Dot42.VStudio.Shared;

namespace Dot42.VStudio.Editors.Menu
{
    [Export(typeof(IXmlEditorPaneContext))]
    internal class XmlMenuEditorPaneContext : IXmlEditorPaneContext
    {
        /// <summary>
        /// Create the appropriate designer control.
        /// </summary>
        public IDesignerControl CreateDesigner(IIde ide, IXmlStore store, IXmlModel model, IServiceProvider serviceProvider, IVsTextLines textBuffer)
        {
            var viewModel = new XmlMenuViewModel(store, model, serviceProvider, textBuffer);
            viewModel.Initialize();
            return new XmlMenuDesignerControl(ide, serviceProvider, viewModel);
        }

        /// <summary>
        /// Is the given MSBuild item type supported by this editor?
        /// </summary>
        public bool SupportsItemType(string itemType)
        {
            switch (itemType)
            {
                case Dot42Constants.ItemTypeMenuResource:
                    return true;
            }
            return false;
        }
    }
}
