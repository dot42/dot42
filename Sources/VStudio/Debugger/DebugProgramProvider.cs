using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    [ComVisible(true)]
    [Guid(GuidList.Strings.guidDot42ProgramProviderClsid)]
    //[Obfuscation] // COM needs a name
    public class DebugProgramProvider : IDebugProgramProvider2
    {
        public int GetProviderProcessData(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA[] pProcess)
        {
            pProcess[0] = new PROVIDER_PROCESS_DATA();
            return VSConstants.S_OK;
        }

        public int GetProviderProgramNode(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode)
        {
            ppProgramNode = null;
            return 0;
        }

        public int WatchForProviderEvents(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback)
        {
            return VSConstants.S_OK;
        }

        public int SetLocale(ushort wLangID)
        {
            return VSConstants.S_OK;
        }
    }
}
