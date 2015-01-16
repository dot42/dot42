using System;
using System.Threading.Tasks;
using Dot42.DeviceLib.UI;

namespace Dot42.VStudio.Flavors
{
    internal static class DeviceExtensions
    {
        /// <summary>
        /// Create a task to start this device.
        /// </summary>
        public static Task Start(this DeviceItem device, Action<string> logger)
        {
            return device.Start(logger);
        }
    }
}
