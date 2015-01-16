using System;
using System.Linq;
using System.Threading.Tasks;
using Dot42.AdbLib;
using Dot42.AvdLib;
using Dot42.FrameworkDefinitions;

namespace Dot42.DeviceLib.UI
{
    public sealed class OfflineEmulatorDeviceItem : DeviceItem
    {
        private readonly Avd avd;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal OfflineEmulatorDeviceItem(Avd avd)
        {
            this.avd = avd;
            ReloadIcon();
            SetName(avd.Name);
            SetSerial(avd.Name);
            SetPlatform(GetPlatform(avd));
            SetCpuAbi(avd.Config.AbiType);
        }

        /// <summary>
        /// Gets the underlying device.
        /// </summary>
        public override IDevice Device { get { return null; } }

        /// <summary>
        /// Gets the emulator description
        /// </summary>
        public Avd Avd { get { return avd; } }

        /// <summary>
        /// Gets the serial of the device.
        /// </summary>
        public override string Serial
        {
            get { return avd.Name; }
        }

        /// <summary>
        /// Gets a unique identifier of this device.
        /// </summary>
        public override string UniqueId
        {
            get { return avd.Name; }
        }

        /// <summary>
        /// Is this device running and connected?
        /// </summary>
        public override bool IsConnected { get { return false; } }

        /// <summary>
        /// Is this device startable?
        /// </summary>
        public override bool CanStart { get { return true; } }

        /// <summary>
        /// Create a task to start this device.
        /// </summary>
        public override Task Start(Action<string> logger)
        {
            var startTask = Task.Factory.StartNew(() => {
                logger("Starting emulator");
                avd.Start(logger);
            });

            // Wait for device
            return startTask.ContinueWith(x => {
                logger("Waiting for emulator");
                new Adb().WaitForDevice(avd.Name, Adb.Timeout.WaitForDevice);
            });
        }

        /// <summary>
        /// Is this device able to run packages that require the given SDK version (API level)
        /// </summary>
        public override bool IsCompatibleWith(int sdkVersion)
        {
            return (avd.TargetApiLevel >= sdkVersion);
        }

        /// <summary>
        /// Gets a human readable platform name
        /// </summary>
        private static string GetPlatform(Avd avd)
        {
            var framework = Frameworks.Instance.FirstOrDefault(x => x.Descriptor.Target == avd.Target);
            return (framework != null) ? framework.ToString() : avd.Target;
        }

        /// <summary>
        /// Reload the image for this item.
        /// </summary>
        protected override void ReloadIcon()
        {
            var index = 5;
            if (!IsCompatible) index += 2;
            ImageIndex = index;
        }
    }
}
