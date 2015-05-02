using System;
using System.Linq;
using Dot42.DebuggerLib.Model;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    /// <summary>
    /// Dalvik specified location
    /// </summary>
    internal sealed class DebugDocumentContext : IDebugDocumentContext2
    {
        private readonly DocumentLocation documentLocation;
        private readonly DebugCodeContext codeContext;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugDocumentContext(DocumentLocation documentLocation, DebugCodeContext codeContext)
        {
            this.documentLocation = documentLocation;
            this.codeContext = codeContext;
            if (codeContext != null) codeContext.DocumentContext = this;
        }

        /// <summary>
        /// Code context contained in me.
        /// </summary>
        public DebugCodeContext CodeContext
        {
            get { return codeContext; }
        }

        public DocumentLocation DocumentLocation
        {
            get { return documentLocation; }
        }

        /// <summary>
        /// Gets the document that contains this document context.
        /// This method is for those debug engines that supply documents directly to the IDE. Since the sample engine
        /// does not do this, this method returns E_NOTIMPL.
        /// </summary>
        public int GetDocument(out IDebugDocument2 ppDocument)
        {
            ppDocument = null;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Gets the displayable name of the document that contains this document context.
        /// </summary>
        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            if (DocumentLocation.SourceCode != null)
            {
                pbstrFileName = DocumentLocation.SourceCode.Document.Path;
                return VSConstants.S_OK;
            }
            pbstrFileName = null;
            return VSConstants.E_FAIL;
        }

        public int EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            ppEnumCodeCxts = new CodeContextEnum(CodeContext != null ? new[] { CodeContext } : Enumerable.Empty<IDebugCodeContext2>());
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the language associated with this document context.
        /// </summary>
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = "C#";
            pguidLanguage = AD7Guids.guidLanguageCSharp;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the file statement range of the document context.
        /// A statement range is the range of the lines that contributed the code to which this document context refers.
        /// </summary>
        public int GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugDocumentContext.GetStatementRange");

            var sourcePosition = DocumentLocation.SourceCode;
            if (sourcePosition == null)
                return VSConstants.E_FAIL;

            var position = sourcePosition.Position;

            // TEXT_POSITION starts counting at 0
            pBegPosition[0].dwLine = (uint) (position.Start.Line - 1);
            pBegPosition[0].dwColumn = (uint) (position.Start.Column - 1);
            pEndPosition[0].dwLine = (uint) (position.End.Line - 1);
            pEndPosition[0].dwColumn = (uint) (position.End.Column - 1);
            DLog.Debug(DContext.VSDebuggerComCall, "Range: {0}-{1}", position.Start, position.End);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the source code range of this document context.
        /// A source range is the entire range of source code, from the current statement back to just after the previous s
        /// statement that contributed code. The source range is typically used for mixing source statements, including 
        /// comments, with code in the disassembly window.
        /// </summary>
        public int GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugDocumentContext.GetSourceRange");

            var sourcePosition = DocumentLocation.SourceCode;
            if (sourcePosition == null)
                return VSConstants.E_FAIL;

            var position = sourcePosition.Position;

            // TEXT_POSITION starts counting at 0
            pBegPosition[0].dwLine = (uint)(position.Start.Line - 1);
            pBegPosition[0].dwColumn = (uint)(position.Start.Column - 1);
            pEndPosition[0].dwLine = (uint)(position.End.Line - 1);
            pEndPosition[0].dwColumn = (uint)(position.End.Column - 1);
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Compares this document context to a given array of document contexts.
        /// </summary>
        public int Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            pdwDocContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        /// <summary>
        /// Moves the document context by a given number of statements or lines.
        /// This is used primarily to support the Autos window in discovering the proximity statements around 
        /// this document context. 
        /// </summary>
        public int Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}
