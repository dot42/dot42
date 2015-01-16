using System;
using Dot42.Utility;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    public class DebugModule : IDebugModule3
    {
        public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugModule.ReloadSymbols_Deprecated");
            throw new NotImplementedException();
        }

        public int GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugModule.GetSymbolInfo");
            throw new NotImplementedException();
        }

        public int LoadSymbols()
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugModule.LoadSymbols");
            throw new NotImplementedException();
        }

        public int IsUserCode(out int pfUser)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugModule.IsUserCode");
            pfUser = 1;
            return VSConstants.S_OK;
        }

        public int SetJustMyCodeState(int fIsUserCode)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "DebugModule.SetJustMyCode");
            throw new NotImplementedException();
        }

        public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            DLog.Debug(DContext.VSDebuggerComCall, "Module.GetInfo");

            pinfo[0].m_addrLoadAddress = 0;
            pinfo[0].m_addrPreferredLoadAddress = 0;
            pinfo[0].m_bstrDebugMessage = "<none>";
            pinfo[0].m_bstrName = Name;
            pinfo[0].m_bstrUrl = "<unknown>";
            pinfo[0].m_bstrUrlSymbolLocation = "<unknown>";
            pinfo[0].m_bstrVersion = "<unknown>";
            pinfo[0].m_dwLoadOrder = 0;
            pinfo[0].m_dwModuleFlags = 0;
            pinfo[0].m_dwSize = 0;
            pinfo[0].dwValidFields = enum_MODULE_INFO_FIELDS.MIF_ALLFIELDS;

            return VSConstants.S_OK;
        }

        public string Name { get { return "<main module>"; } }
    }
}
