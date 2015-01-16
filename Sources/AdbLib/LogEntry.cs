using System;

namespace Dot42.AdbLib
{
    public class LogEntry
    {
        private const int EntryHeaderSize = 20; // 2*2 + 4*4; see LogEntry.

        /// <summary>
        ///  generating process's process id
        /// </summary>
        public int Pid;

        /// <summary>
        /// generating process's tid
        /// </summary>
        public int Tid;

        /// <summary>
        ///  seconds since Epoch
        /// </summary>
        public int Seconds;

        /// <summary>
        /// nanoseconds
        /// </summary>
        public int NanoSeconds;

        /// <summary>
        /// The entry's payload 
        /// </summary>
        public byte[] Message;

        private LogLevel level;
        private string tag;
        private string messageStr;

        /// <summary>
        /// Event time
        /// </summary>
        public DateTime Time
        {
            get
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return epoch.AddSeconds(Seconds);
            }
        }

        /// <summary>
        /// Level of this entry
        /// </summary>
        public LogLevel Level
        {
            get { ParseMessage(); return level; }
        }

        /// <summary>
        /// Gets the payload as a string
        /// </summary>
        public string MessageAsString
        {
            get { ParseMessage(); return messageStr; }
        }

        /// <summary>
        /// Used to identify the source of the message
        /// </summary>
        public string Tag
        {
            get { ParseMessage(); return tag; }
        }

        /// <summary>
        /// Parse the message data into its components
        /// </summary>
        private void ParseMessage()
        {
            if (messageStr != null)
                return;

            var value = (Message.Length > 0) ? Message[0] : -1;
            var offset = 0;
            if ((value >= (int)LogLevel.MinValue) && (value <= (int)LogLevel.MaxValue))
            {
                level = (LogLevel) value;
                offset++;
            }

            var str = AdbRequest.DefaultEncoding.GetString(Message, offset, Message.Length - offset);
            var index = str.IndexOf('\0');
            tag = (index > 0) ? str.Substring(0, index) : string.Empty;
            messageStr = (index > 0) ? str.Substring(index + 1) : str;
            messageStr = messageStr.Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Try to parse an entry from the given data.
        /// </summary>
        internal static bool TryParse(byte[] data, int offset, int length, out LogEntry entry, out int entryLength)
        {
            entry = null;
            entryLength = 0;

            // Enough data for the header?
            if (offset + EntryHeaderSize > length)
                return false;

            // Get payload length
            // Enough data for the header and payload?
            var len = ArrayHelper.SwapU16BitFromArray(data, offset + entryLength);
            if (offset + EntryHeaderSize + len > length)
                return false;

            // we've read only 16 bits, but since there's also a 16 bit padding,
            // we can skip right over both.
            entryLength += 4;

            entry = new LogEntry();
            entry.Pid = ArrayHelper.Swap32BitFromArray(data, offset + entryLength);
            entryLength += 4;
            entry.Tid = ArrayHelper.Swap32BitFromArray(data, offset + entryLength);
            entryLength += 4;
            entry.Seconds = ArrayHelper.Swap32BitFromArray(data, offset + entryLength);
            entryLength += 4;
            entry.NanoSeconds= ArrayHelper.Swap32BitFromArray(data, offset + entryLength);
            entryLength += 4;

            entry.Message = new byte[len];
            Array.Copy(data, offset + entryLength, entry.Message, 0, len);
            entryLength += len;

            return true;
        }
    }
}
