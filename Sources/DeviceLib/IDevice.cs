namespace Dot42.DeviceLib
{
    /// <summary>
    /// Single device.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Gets device serial.
        /// </summary>
        string Serial { get; }

        /// <summary>
        /// Gets device password (null if not supported).
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Human readable name of this device
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a unique ID for this device.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// Gets the state of this device.
        /// </summary>
        string State { get; }

        /// <summary>
        /// Is this an emulator?
        /// </summary>
        bool IsEmulator { get; }

        /// <summary>
        /// Is this device able to run packages that require the given SDK version (API level)
        /// </summary>
        bool IsCompatibleWith(int sdkVersion);

        /// <summary>
        /// Gets the BuildVersionSdk of this device.
        /// </summary>
        int SdkVersion { get; }
    }
}