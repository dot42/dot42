using Dot42.DeviceLib;

namespace Dot42.BarDeployLib
{
    /// <summary>
    /// BlackBerry device identification
    /// </summary>
    public class BlackBerryDevice : IDevice
    {
        private const int BlackBerrySdkVersion = 17;

        private readonly string deviceIp;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal BlackBerryDevice(string deviceIp, string password)
        {
            this.deviceIp = deviceIp;
            Password = password;
        }

        /// <summary>
        /// Gets device serial.
        /// </summary>
        public string Serial { get { return deviceIp + ":5555"; } }

        /// <summary>
        /// Gets device password (null if not supported).
        /// </summary>
        public string Password { get; internal set; }

        /// <summary>
        /// Human readable name of this device
        /// </summary>
        public string Name { get { return deviceIp; } }

        /// <summary>
        /// Gets a unique ID for this device.
        /// </summary>
        public string UniqueId { get { return deviceIp; } }

        /// <summary>
        /// Gets the state of this device.
        /// </summary>
        public string State { get { return "Network"; } }

        /// <summary>
        /// Is this an emulator?
        /// </summary>
        public bool IsEmulator { get { return false; } }

        /// <summary>
        /// Is this device able to run packages that require the given SDK version (API level)
        /// </summary>
        public bool IsCompatibleWith(int sdkVersion)
        {
            return (sdkVersion <= BlackBerrySdkVersion);
        }

        /// <summary>
        /// Gets the BuildVersionSdk of this device.
        /// </summary>
        public int SdkVersion { get { return BlackBerrySdkVersion; } }

        /// <summary>
        /// Use for change detection.
        /// </summary>
        internal string Hash { get { return deviceIp + Password; }}
    }
}
