using System;

namespace Dot42.AdbLib
{
    /// <summary>
    /// Helper used to receive an parse log data.
    /// </summary>
    internal sealed class LogOutputReceiver
    {
        private readonly ILogListener listener;
        private byte[] buffer;
        private int bufferLength;

        /// <summary>
        /// Default ctor
        /// </summary>
        public LogOutputReceiver(ILogListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException("listener");
            this.listener = listener;
        }

        /// <summary>
        /// Add received output data.
        /// </summary>
        internal void AddOutput(byte[] data, int offset, int length)
        {
            // Calculate required buffer length
            var reqBufferLength = bufferLength + length;

            // Do we need to enlarge the buffer?
            if ((buffer == null) || (reqBufferLength > buffer.Length))
            {
                var newData = new byte[reqBufferLength];
                if (bufferLength > 0)
                    Array.Copy(buffer, 0, newData, 0, bufferLength);
                buffer = newData;
            }

            // Append data to buffer
            Array.Copy(data, offset, buffer, bufferLength, length);
            bufferLength += length;

            // Try to parse from the buffer.
            var bufferOffset = 0;
            while (true)
            {
                LogEntry entry;
                int entryLength;
                if (LogEntry.TryParse(buffer, bufferOffset, bufferLength, out entry, out entryLength))
                {
                    // Parsed successfully.
                    // Update my settings
                    bufferLength -= entryLength;
                    bufferOffset += entryLength;
                    // Pass entry onto listener
                    listener.AddEntry(entry);
                }
                else
                {
                    // Not enough data remaining
                    break;
                }
            }

            if (bufferLength > 0)
            {
                // Move data to beginning of buffer
                Array.Copy(buffer, bufferOffset, buffer, 0, bufferLength);                
            }
        }

        /// <summary>
        /// Should further processing be cancelled?
        /// </summary>
        public bool IsCancelled { get { return listener.IsCancelled; } }
    }
}
