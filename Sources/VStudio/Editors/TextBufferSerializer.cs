using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Dot42.VStudio.Editors
{
    /// <summary>
    /// Serialize/deserialize XML to/from IVsTextLines
    /// </summary>
    internal sealed class TextBufferSerializer
    {
        private readonly IVsTextLines textBuffer;

        /// <summary>
        /// Default ctor
        /// </summary>
        public TextBufferSerializer(IVsTextLines textBuffer)
        {
            this.textBuffer = textBuffer;
        }

        /// <summary>
        /// Parse the contents of the buffer.
        /// </summary>
        public XDocument Parse()
        {
            var xml = GetAllText();
            return XDocument.Parse(xml);
        }

        /// <summary>
        /// Overwrite the text in the buffer with the given document
        /// </summary>
        public void Save(XDocument document)
        {
            var xml = document.ToString(SaveOptions.None);
            SetText(xml);
        }

        /// <summary>
        /// Gets the entire text from the buffer.
        /// </summary>
        private string GetAllText()
        {
            var span = new TextSpan();
            textBuffer.GetLastLineIndex(out span.iEndLine, out span.iEndIndex);
            string text;
            textBuffer.GetLineText(span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex, out text); 
            return text;
        }

        /// <summary>
        /// Overwrite the text in the buffer with the given text.
        /// </summary>
        private void SetText(string text)
        {
            int endLine, endCol;
            textBuffer.GetLastLineIndex(out endLine, out endCol);
            var len = (text == null) ? 0 : text.Length;
            //fix location of the string
            var pText = Marshal.StringToCoTaskMemAuto(text);
            try
            {
                textBuffer.ReplaceLines(0, 0, endLine, endCol, pText, len, null);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pText);
            }
        }
    }
}
