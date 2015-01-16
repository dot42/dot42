using System;
using System.Threading.Tasks;
using Dot42.AdbLib;

namespace Dot42.DeviceLib.UI
{
    public sealed class AndroidDeviceItem : DeviceItem
    {
        private readonly AndroidDevice device;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AndroidDeviceItem(AndroidDevice device)
        {
            this.device = device;
            ReloadIcon();
            SetName(device.Name);
            SetType(device.IsEmulator ? "Emulator" : "Device");
            SetSerial(Serial);
            SetPlatform(device.Platform);
            SetCpuAbi(device.ProductCpuAbi);
            SetState(((IDevice)device).State);
        }

        /// <summary>
        /// Gets the underlying device.
        /// </summary>
        public override IDevice Device { get { return device; } }

        /// <summary>
        /// Gets the serial of the device.
        /// </summary>
        public override string Serial
        {
            get { return device.Serial; }
        }

        /// <summary>
        /// Gets a unique identifier of this device.
        /// </summary>
        public override string UniqueId
        {
            get { return device.IsEmulator ? device.Name : device.Serial; }
        }

        /// <summary>
        /// Is this device running and connected?
        /// </summary>
        public override bool IsConnected { get { return true; } }

        /// <summary>
        /// Is this device startable?
        /// </summary>
        public override bool CanStart { get { return false; } }

        /// <summary>
        /// Create a task to start this device.
        /// </summary>
        public override Task Start(Action<string> logger)
        {
            return Task.Factory.StartNew(() => {});
        }

        /// <summary>
        /// Is this device able to run packages that require the given SDK version (API level)
        /// </summary>
        public override bool IsCompatibleWith(int sdkVersion)
        {
            return device.IsCompatibleWith(sdkVersion);
        }

        /// <summary>
        /// Reload the image for this item.
        /// </summary>
        protected override void ReloadIcon()
        {
            var index = device.IsEmulator ? 4 : 0;
            if (!IsCompatible) index += 2;
            ImageIndex = index;
        }
    }
}
