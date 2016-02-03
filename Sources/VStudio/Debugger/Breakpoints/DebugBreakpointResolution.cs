using System.Runtime.InteropServices;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugBreakpointResolution : IDebugBreakpointResolution2
    {
        private readonly enum_BP_TYPE type;
        private readonly DebugProgram program;
        private readonly DalvikLocationBreakpoint locationBP;

        /// <summary>
        /// breakpoint can be null.
        /// </summary>
        public DebugBreakpointResolution(enum_BP_TYPE type, DebugProgram program, DalvikLocationBreakpoint locationBP)
        {
            this.type = type;
            this.program = program;
            this.locationBP = locationBP;
        }

        /// <summary>
        /// Current thread
        /// </summary>
        public DebugThread Thread { get; set; }

        public int GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            pBPType[0] = type;
            return VSConstants.S_OK;
        }

        public int GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo)
        {
            var info = new BP_RESOLUTION_INFO();
            info.pProgram = program;
            info.dwFields = enum_BPRESI_FIELDS.BPRESI_PROGRAM;

            if (Thread != null)
            {
                info.pThread = Thread;
                info.dwFields |= enum_BPRESI_FIELDS.BPRESI_THREAD;
            }

            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != 0 && locationBP != null)
            {
                info.bpResLocation.bpType = (uint)enum_BP_TYPE.BPT_CODE;

                var codeContext = new DebugCodeContext(locationBP.Location);
                var docLoc = locationBP.DocumentLocation;
                if(docLoc != null) codeContext.DocumentContext = new DebugDocumentContext(docLoc, codeContext);

                // The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
                // to IDebugCodeContex2 and not IUnknown.
                info.bpResLocation.unionmember1 = Marshal.GetComInterfaceForObject(codeContext, typeof(IDebugCodeContext2));
                info.dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
            }

            pBPResolutionInfo[0] = info;
            return VSConstants.S_OK;
        }
    }
}
