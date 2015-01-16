using System;
using Dot42.DebuggerLib;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugCodeContext : IDebugCodeContext2
    {
        private readonly Location location;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugCodeContext(Location location)
        {
            this.location = location;
        }

        /// <summary>
        /// Document context describing this location.
        /// </summary>
        internal DebugDocumentContext DocumentContext { get; set; }

        /// <summary>
        /// Gets the user-displayable name for this context
        /// This is not supported by the sample engine.
        /// </summary>
        public int GetName(out string pbstrName)
        {
            pbstrName = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            var info = new CONTEXT_INFO();
            info.dwFields = 0;
            pinfo[0] = info;
            return VSConstants.S_OK;
        }

        public int Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            pdwMemoryContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = DocumentContext;
            return VSConstants.S_OK;
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            var docCtx = DocumentContext;
            return (docCtx != null) ? docCtx.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage) : VSConstants.E_FAIL;
        }
    }
}
