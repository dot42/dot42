namespace Dot42.DebuggerLib
{
    /// <summary>
    /// VM version data.
    /// </summary>
    public sealed class VersionInfo
    {
        public readonly string Description;
        public readonly int JdwpMajorVersion;
        public readonly int JdwpMinorVersion;
        public readonly string VmJreVersion;
        public readonly string VmName;

        public VersionInfo(string description, int jdwpMajorVersion, int jdwpMinorVersion, string vmJreVersion, string vmName)
        {
            Description = description;
            JdwpMajorVersion = jdwpMajorVersion;
            JdwpMinorVersion = jdwpMinorVersion;
            VmJreVersion = vmJreVersion;
            VmName = vmName;
        }
    }
}
