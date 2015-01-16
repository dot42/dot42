namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Status information about a thread.
    /// </summary>
    public sealed class ThreadStatusInfo
    {
        public readonly Jdwp.ThreadStatus ThreadStatus;
        public readonly Jdwp.SuspendStatus SuspendStatus;

        public ThreadStatusInfo(Jdwp.ThreadStatus threadStatus, Jdwp.SuspendStatus suspendStatus)
        {
            ThreadStatus = threadStatus;
            SuspendStatus = suspendStatus;
        }
    }
}
