using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dot42.DeviceLib.UI
{
    /// <summary>
    /// Device list item
    /// </summary>
    public abstract class DeviceItem : ListViewItem
    {
        private bool isCompatible;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected DeviceItem()
        {
            SubItems.Add("");
            SubItems.Add("");
            SubItems.Add("");
            SubItems.Add("");
            SubItems.Add("");
        }

        /// <summary>
        /// Name column
        /// </summary>
        protected void SetName(string columnValue)
        {
            Text = columnValue;
        }

        /// <summary>
        /// Type column
        /// </summary>
        protected void SetType(string columnValue)
        {
            SubItems[1].Text = columnValue;
        }

        /// <summary>
        /// Serial column
        /// </summary>
        protected void SetSerial(string columnValue)
        {
            SubItems[2].Text = columnValue;
        }

        /// <summary>
        /// Platform column
        /// </summary>
        protected void SetPlatform(string columnValue)
        {
            SubItems[3].Text = columnValue;
        }

        /// <summary>
        /// CPU/Abicolumn
        /// </summary>
        protected void SetCpuAbi(string columnValue)
        {
            SubItems[4].Text = columnValue;
        }

        /// <summary>
        /// State column
        /// </summary>
        protected void SetState(string columnValue)
        {
            SubItems[5].Text = columnValue;
        }

        /// <summary>
        /// Gets the running device.
        /// Can be null.
        /// </summary>
        public abstract IDevice Device { get; }

        /// <summary>
        /// Gets the name of the running device.
        /// </summary>
        public string DeviceName
        {
            get
            {
                var dev = Device;
                return (dev != null) ? dev.Name : "?";
            }
        }

        /// <summary>
        /// Gets the serial of the device.
        /// </summary>
        public abstract string Serial { get; }

        /// <summary>
        /// Is this device running and connected?
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Gets a unique identifier of this device.
        /// </summary>
        public abstract string UniqueId { get; }

        /// <summary>
        /// Is this device startable?
        /// </summary>
        public abstract bool CanStart { get; }

        /// <summary>
        /// Is this device compatible with our intended task?
        /// This value is used in the icon used to represent this item.
        /// </summary>
        public bool IsCompatible
        {
            get { return isCompatible; }
            set
            {
                if (isCompatible != value)
                {
                    isCompatible = value;
                    ReloadIcon();
                }
            }
        }

        /// <summary>
        /// Reload the image for this item.
        /// </summary>
        protected abstract void ReloadIcon();

        /// <summary>
        /// Create a task to start this device.
        /// </summary>
        public abstract Task Start(Action<string> logger);

        /// <summary>
        /// Is this device able to run packages that require the given SDK version (API level)
        /// </summary>
        public abstract bool IsCompatibleWith(int sdkVersion);
    }
}
