namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Response to a HELO command.
    /// </summary>
    public sealed class HeloInfo
    {
        public readonly int ClientProtocolVersion;
        public readonly int ProcessId;
        public readonly string VmIdentifier;
        public readonly string AppName;

        public HeloInfo(int clientProtocolVersion, int processId, string vmIdentifier, string appName)
        {
            ClientProtocolVersion = clientProtocolVersion;
            ProcessId = processId;
            VmIdentifier = vmIdentifier;
            AppName = appName;
        }
    }
}
