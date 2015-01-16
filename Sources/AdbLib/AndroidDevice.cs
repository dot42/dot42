using System;
using System.Collections.Generic;
using System.Text;
using Dot42.DeviceLib;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Single device.
    /// </summary>
    public class AndroidDevice : IDevice, IEquatable<AndroidDevice>
    {
        private readonly string serial;
        private readonly AndroidDeviceStates state;
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>();
        private string name;
        private string platform;

        /// <summary>
        /// Default ctor
        /// </summary>
        public AndroidDevice(string serial, AndroidDeviceStates state)
        {
            this.serial = serial;
            this.state = state;
        }

        /// <summary>
        /// Gets a unique ID for this device.
        /// </summary>
        public string UniqueId
        {
            get { return serial; }
        }

        /// <summary>
        /// State of the device.
        /// </summary>
        public AndroidDeviceStates DeviceState
        {
            get { return state; }
        }

        /// <summary>
        /// Gets the state of this device.
        /// </summary>
        string IDevice.State
        {
            get
            {
                switch (DeviceState)
                {
                    case AndroidDeviceStates.Offline:
                        return Resources.StateOffline;
                    case AndroidDeviceStates.Device:
                        if (IsEmulator)
                        {
                            string status;
                            var port = Emulator.GetPort(serial);
                            if (Emulator.TryGetAvdStatus(port, out status))
                                return status;
                        }
                        return Resources.StateDevice;
                    default:
                        throw new ArgumentException(string.Format("Unknown device state {0}", (int)state));
                }
            }
        }

        /// <summary>
        /// Gets device serial.
        /// </summary>
        public string Serial
        {
            get { return serial; }
        }

        /// <summary>
        /// Gets device password (null if not supported).
        /// </summary>
        public string Password
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a complete name.
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null)
                {
                    var sb = new StringBuilder();
                    if (IsEmulator)
                    {
                        var port = Emulator.GetPort(serial);
                        if (!Emulator.TryGetAvdName(port, out name))
                        {
                            return serial;
                        }
                    }
                    else
                    {
                        var x = ProductModel;
                        if (!string.IsNullOrEmpty(x)) sb.AppendFormat("{0} ", x);
                        x = ProductManufacturer;
                        if (!string.IsNullOrEmpty(x)) sb.AppendFormat("[{0}] ", x);
                        // Use serial if not properties found
                        name = (sb.Length == 0) ? Serial : sb.ToString().Trim();
                    }
                }
                return name;
            }
        }

        /// <summary>
        /// Gets a complete platform name the device is using.
        /// </summary>
        public string Platform
        {
            get
            {
                if (platform == null)
                {
                    var sb = new StringBuilder();
                    var x = BuildVersionRelease;
                    if (!string.IsNullOrEmpty(x)) sb.AppendFormat("{0} ", x);
                    x = BuildVersionSdk;
                    if (!string.IsNullOrEmpty(x)) sb.AppendFormat("SDK {0} ", x);
                    // Use serial if not properties found
                    platform = sb.ToString().Trim();
                }
                return platform;
            }
        }

        /// <summary>
        /// Gets the name of the products manufacturer
        /// </summary>
        public string ProductManufacturer
        {
            get { return GetProperty("ro.product.manufacturer"); }
        }

        /// <summary>
        /// Gets the name of the products model
        /// </summary>
        public string ProductModel
        {
            get { return GetProperty("ro.product.model"); }
        }

        /// <summary>
        /// Gets the name of the products cpu ABI
        /// </summary>
        public string ProductCpuAbi
        {
            get { return GetProperty("ro.product.cpu.abi"); }
        }

        /// <summary>
        /// Gets the name of the products cpu ABI (2)
        /// </summary>
        public string ProductCpuAbi2
        {
            get { return GetProperty("ro.product.cpu.abi2"); }
        }

        /// <summary>
        /// Gets the name of the release the device is running
        /// </summary>
        public string BuildVersionRelease
        {
            get { return GetProperty("ro.build.version.release"); }
        }

        /// <summary>
        /// Gets the number of the SDK the device is running
        /// </summary>
        public string BuildVersionSdk
        {
            get { return GetProperty("ro.build.version.sdk"); }
        }

        /// <summary>
        /// Is this an emulator?
        /// </summary>
        public bool IsEmulator
        {
            get
            {
                return (Emulator.GetPort(serial) > 0);
                //return (GetProperty("ro.kernel.qemu") == "1");
            }
        }

        /// <summary>
        /// Is this device able to run packages that require the given SDK version (API level)
        /// </summary>
        public bool IsCompatibleWith(int sdkVersion)
        {
            int buildSdk;
            if (!int.TryParse(BuildVersionSdk, out buildSdk))
                return false;
            return (buildSdk >= sdkVersion);
        }

        /// <summary>
        /// Gets the BuildVersionSdk of this device.
        /// </summary>
        public int SdkVersion
        {
            get
            {
                int buildSdk;
                if (!int.TryParse(BuildVersionSdk, out buildSdk))
                    return -1;
                return buildSdk;
            }
        }

        /// <summary>
        /// Gets a (cached) property from the device.
        /// </summary>
        private string GetProperty(string propertyName)
        {
            string result;
            if (properties.TryGetValue(propertyName, out result))
                return result;
            result = Adb.GetProperty(this, propertyName, Adb.Timeout.GetProperty, false);
            properties[propertyName] = result;
            return result;
        }

        /// <summary>
        /// Is this equal to other?
        /// The serial and the state are compared.
        /// </summary>
        public bool Equals(AndroidDevice other)
        {
            return (other != null) &&
                   (serial == other.serial) &&
                   (state == other.state);
        }

        /// <summary>
        /// Is this equal to other?
        /// </summary>
        public override bool Equals(object other)
        {
            return Equals(other as AndroidDevice);
        }

        /// <summary>
        /// Gets hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return serial.GetHashCode() ^ (int) state;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
            return serial;
        }
    }
}
