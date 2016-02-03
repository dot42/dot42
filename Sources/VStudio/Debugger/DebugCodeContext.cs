using System;
using Dot42.DebuggerLib;
using Dot42.Mapping;
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

        public Location Location { get { return location; } }

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

            int ret = VSConstants.S_FALSE;
            info.dwFields = 0;

            var loc = DocumentContext == null ? null : DocumentContext.DocumentLocation;
            if (loc == null)
            {
                ret = VSConstants.E_FAIL;
            }
            else
            {
                if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0)
                {
                    info.bstrFunction = loc.MethodName;
                    info.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION;
                }

                if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET) != 0)
                {
                    if (loc.SourceCode != null && !loc.SourceCode.IsSpecial)
                    {
                        info.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET;
                        info.posFunctionOffset.dwLine = (uint) loc.SourceCode.Position.MethodOffset;
                    }
                }

                if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0)
                {
                    info.bstrAddress = loc.Location.Index.ToString("X3").PadLeft(4);
                    info.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
                }

                if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0)
                {
                    info.bstrModuleUrl = "<default>";
                    info.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL;
                }

                if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0)
                {
                    info.bstrAddressAbsolute = loc.Location.ToString();
                    info.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE;
                }
            }

            pinfo[0] = info;
            return ret;
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
            
            for(int i = 0; i < dwMemoryContextSetLen; ++i)
            {
                pdwMemoryContext = (uint)i;
                var other = rgpMemoryContextSet[i] as DebugCodeContext;
                if (other == null)
                    return HResults.E_COMPARE_CANNOT_COMPARE;

                var isSameMethod = Equals(Location.Class, other.Location.Class) && Equals(Location.Method, other.Location.Method);

                switch (Compare)
                {
                    case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                        if (location.Equals(other.Location))
                            return VSConstants.S_OK;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                        if (!isSameMethod) return HResults.E_COMPARE_CANNOT_COMPARE;

                        if(Location.CompareTo(other.Location) > 0)
                            return VSConstants.S_OK;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                        if (!isSameMethod) return HResults.E_COMPARE_CANNOT_COMPARE;
                        if (Location.CompareTo(other.Location) >= 0)
                            return VSConstants.S_OK;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                        if (!isSameMethod) return HResults.E_COMPARE_CANNOT_COMPARE;
                        if (Location.CompareTo(other.Location) < 0)
                            return VSConstants.S_OK;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                        if (!isSameMethod) return HResults.E_COMPARE_CANNOT_COMPARE;
                        if (Location.CompareTo(other.Location) <= 0)
                            return VSConstants.S_OK;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                        return VSConstants.S_OK;
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                        if(isSameMethod)
                            return VSConstants.S_OK;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                        // TODO: analyze scopes.
                        if(isSameMethod)
                            return VSConstants.S_OK;
                        break;
                }
            }

            pdwMemoryContext = (uint)0;
            return VSConstants.S_FALSE;
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
