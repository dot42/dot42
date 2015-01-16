using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using VSXmlLanguageService = Microsoft.XmlEditor.XmlLanguageService;

namespace Dot42.VStudio.XmlEditor
{
    internal class XmlLanguageService : IXmlLanguageService
    {
        private readonly VSXmlLanguageService languageService;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlLanguageService(LanguageService languageService)
        {
            this.languageService = (VSXmlLanguageService) languageService;
        }

        public bool IsParsing
        {
            get { return languageService.IsParsing; }
            set { languageService.IsParsing = value; }
        }

        public ISource GetSource(IVsTextLines textBuffer)
        {
            var src = languageService.GetSource(textBuffer);
            if (src == null)
                return null;
            return new Source(src);
        }

        public IXmlDocument GetParseTree(ISource source, IVsTextView vsTextView, int line, int column)
        {
            var doc = languageService.GetParseTree(((Source) source).source, vsTextView, line, column, ParseReason.CompleteWord);
            return (doc != null) ? new XmlDocument(doc) : null;
        }

        public bool AutoInsertAttributeQuotes
        {
            get { return languageService.XmlPrefs.AutoInsertAttributeQuotes; }
        }
    }
}
