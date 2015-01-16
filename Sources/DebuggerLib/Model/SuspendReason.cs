namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Why did the process suspend?
    /// </summary>
    public enum SuspendReason
    {
        /// <summary>
        /// Entire process was asked to suspend
        /// </summary>
        ProcessSuspend,

        /// <summary>
        /// A breakpoint occurred
        /// </summary>
        Breakpoint,

        /// <summary>
        /// A step has completed
        /// </summary>
        SingleStep,

        /// <summary>
        /// An exception has occurred
        /// </summary>
        Exception
    }
}
