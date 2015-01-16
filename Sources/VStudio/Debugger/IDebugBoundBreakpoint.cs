using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal interface IDebugBoundBreakpoint : IDebugBoundBreakpoint2
    {
        /// <summary>
        /// Gets the contained breakpoint
        /// </summary>
        DalvikBreakpoint Breakpoint { get; }

        /// <summary>
        /// Called when this breakpoint has been reset.
        /// </summary>
        void OnReset();
    }
}
