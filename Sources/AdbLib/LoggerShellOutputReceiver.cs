using System;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Pass shell output through to a log callback.
    /// </summary>
    public class LoggerShellOutputReceiver : IShellOutputReceiver
    {
        private readonly Action<string> logger;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LoggerShellOutputReceiver(Action<string> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Add received output data.
        /// </summary>
        public void AddOutput(byte[] data, int offset, int length)
        {
            if (logger != null)
            {
                logger(AdbRequest.DefaultEncoding.GetString(data, offset, length));                
            }
        }

        /// <summary>
        /// Command has finished.
        /// </summary>
        public void Completed()
        {
        }

        /// <summary>
        /// Should further processing be cancelled?
        /// </summary>
        public bool IsCancelled { get { return false; }}
    }
}
