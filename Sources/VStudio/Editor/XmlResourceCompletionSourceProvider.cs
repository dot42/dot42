using System.ComponentModel.Composition;
using System.IO;
using Dot42.Ide.Project;
using Dot42.VStudio.Shared;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Dot42.VStudio.Editor
{
    /// <summary>
    /// Completion source provider for android XML resources.
    /// </summary>
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("XML")]
    [Order(After = "default")]
    [Name("Layout completion")]
    internal class XmlResourceCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal IGlyphService GlyphService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        internal IVsEditorAdaptersFactoryService VsEditorAdaptersFactoryService { get; set; }

        /// <summary>
        /// Creates a completion provider for the given context.
        /// </summary>
        /// <param name="textBuffer">The text buffer over which to create a provider.</param>
        /// <returns>
        /// A valid <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSource"/> instance, or null if none could be created.
        /// </returns>
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            var itemType = textBuffer.GetProjectItemType(ServiceProvider);

            switch (itemType)
            {
                case Dot42Constants.ItemTypeLayoutResource:
                    return new LayoutResourceCompletionSource(textBuffer, ServiceProvider, VsEditorAdaptersFactoryService, GlyphService);
                case Dot42Constants.ItemTypeMenuResource:
                default:
                    return new MenuResourceCompletionSource(textBuffer, ServiceProvider, VsEditorAdaptersFactoryService, GlyphService);
                /*default:
                    return null;*/
            }
        }
    }
}
