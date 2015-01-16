using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Dot42.VStudio.Editor
{
    /// <summary>
    /// Provider for XML Resource Completion Controller.
    /// </summary>
    [Export(typeof (IVsTextViewCreationListener))]
    [Name("Dot42 XML Resource Completion Controller")]
    [ContentType("xml")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class XmlResourceCompletionControllerProvider
        :
            IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelectorService { get; set; }

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = VsEditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (textView == null)
            {
                return;
            }

            Func<XmlResourceCompletionController> controllerCreator =
                () => new XmlResourceCompletionController(ServiceProvider, textViewAdapter, textView, CompletionBroker, TextStructureNavigatorSelectorService);
            textView.Properties.GetOrCreateSingletonProperty(controllerCreator);
        }

    }
}
