using System;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Dot42.VStudio.XmlEditor
{
    public interface IXmlLanguageService
    {
        bool IsParsing { get; set; }
        ISource GetSource(IVsTextLines buffer);
        IXmlDocument GetParseTree(ISource source, IVsTextView vsTextView, int line, int column);
        bool AutoInsertAttributeQuotes { get; }
    }
}
