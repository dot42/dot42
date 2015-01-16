using Dot42.Ide.Descriptors;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace Dot42.VStudio.Editor
{
    /// <summary>
    /// Completion source for layout XML's
    /// </summary>
    internal sealed class LayoutResourceCompletionSource : XmlResourceCompletionSource
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public LayoutResourceCompletionSource(ITextBuffer textBuffer, SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService, IGlyphService glyphService)
            : base(textBuffer, serviceProvider, vsEditorAdaptersFactoryService, glyphService)
        {
        }

        /// <summary>
        /// Gets the descriptor provider to use for this kind of resources.
        /// </summary>
        /// <returns></returns>
        protected override DescriptorProvider GetDescriptorProvider(DescriptorProviderSet providerSet)
        {
            return providerSet.LayoutDescriptors;
        }
    }
}
