using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugBreakpointResolution : IDebugBreakpointResolution2
    {
        private readonly enum_BP_TYPE type;
        private readonly DebugProgram program;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugBreakpointResolution(enum_BP_TYPE type, DebugProgram program)
        {
            this.type = type;
            this.program = program;
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

            pBPResolutionInfo[0] = info;
            return VSConstants.S_OK;
        }
    }
}
