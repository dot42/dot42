namespace Dot42.VStudio.Debugger
{
    internal interface IDebugBreakpoint
    {
        /// <summary>
        /// Gets the containing bound breakpoint.
        /// </summary>
        IDebugBoundBreakpoint BoundBreakpoint { get; }
    }
}
