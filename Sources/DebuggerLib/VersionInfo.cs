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

        public bool IsDalvikVm
        {
            // not so hot to use string comparison here, but looking at the values, this seems to be the
            // reliable way.
            get { return Description != null && Description.StartsWith("Android DalvikVM 1"); }
        }
    }
}
