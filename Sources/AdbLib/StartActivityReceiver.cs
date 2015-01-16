using Dot42.DeviceLib;

namespace Dot42.AdbLib
{
    internal sealed class StartActivityReceiver : IShellOutputReceiver
    {
        private readonly IStartActivityListener listener;

        /// <summary>
        /// Default ctor
        /// </summary>
        public StartActivityReceiver(IStartActivityListener listener)
        {
            this.listener = listener;
        }

        /// <summary>
        /// Add received output data.
        /// </summary>
        public void AddOutput(byte[] data, int offset, int length)
        {
            var msg = AdbRequest.DefaultEncoding.GetString(data, offset, length);
        }

        /// <summary>
        /// Command has finished.
        /// </summary>
        public void Completed()
        {
            if (listener != null)
            {
                listener.Completed();
            }
        }

        /// <summary>
        /// Should further processing be cancelled?
        /// </summary>
        public bool IsCancelled
        {
            get { return (listener != null) && listener.IsCancelled; }
        }
    }
}
