using System;
using System.IO;
using System.Text;
using Dot42.Utility;

namespace Dot42.DebuggerLib
{
    public partial class JdwpPacket
    {
        private static readonly BigEndianBitConverter converter = new BigEndianBitConverter();

        internal static readonly JdwpPacket VmDeadError = new JdwpPacket(null, new byte[HeaderLength], 0) { ErrorCode = Jdwp.ErrorCodes.VM_DEAD };

        internal const int HeaderLength = 11;
        private const int ReplyMask = 0x80;
        private readonly IJdwpServerInfo serverInfo;
        private readonly byte[] data;
        private readonly int dataOffset;
        private DataReaderWriter readerWriter;

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpPacket(IJdwpServerInfo serverInfo, byte[] data, int dataOffset)
            : this(serverInfo, data, dataOffset, false)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public JdwpPacket(IJdwpServerInfo serverInfo, byte[] data, int dataOffset, bool cloneBuffer)
        {
            this.serverInfo = serverInfo;
            this.data = data;
            this.dataOffset = dataOffset;

            if (cloneBuffer)
            {
                var len = Length;
                var clone = new byte[len];
                Array.Copy(data, dataOffset, clone, 0, len);
                this.data = clone;
                this.dataOffset = 0;
            }
        }

        /// <summary>
        /// Create a command packet.
        /// </summary>
        public static JdwpPacket CreateCommand(IJdwpServerInfo serverInfo, int commandSet, int command, int dataLength, Action<JdwpPacket> initialize = null)
        {
            var data = new byte[HeaderLength + dataLength];
            var packet = new JdwpPacket(serverInfo, data, 0) { CommandSet = commandSet, Command = command, Length = data.Length };
            if (initialize != null)
            {
                initialize(packet);
            }
            return packet;
        }

        /// <summary>
        /// Gets this packet as DDMS Chunk.
        /// </summary>
        internal virtual Chunk AsChunk()
        {
            return new Chunk(serverInfo, data, dataOffset);
        }

        /// <summary>
        /// Is this packet actually a DDMS chunk?
        /// </summary>
        internal bool IsChunk()
        {
            if ((CommandSet == Chunk.DdmsCommandSet) && (Command == Chunk.DdmsCommand))
                return true;
            if (IsReply && (CommandSet == 0) && (Command == 0))
            {
                var len = Length;
                if (len >= HeaderLength + Chunk.ChunkHeaderLength)
                {
                    var chunkLen = GetInt32(HeaderLength + 4);
                    if (len == chunkLen + HeaderLength + Chunk.ChunkHeaderLength)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Length of the entire packet
        /// </summary>
        public int Length
        {
            get { return GetInt32(0); }
            set { SetInt32(0, value);}
        }

        /// <summary>
        /// Unique identification of this packet.
        /// </summary>
        public int Id
        {
            get { return GetInt32(4); }
            set { SetInt32(4, value); }
        }

        /// <summary>
        /// Flags
        /// </summary>
        public int Flags
        {
            get { return GetByte(8); }
            set { SetByte(8, (byte) value); }
        }

        /// <summary>
        /// Is the reply bit set in the flags?
        /// </summary>
        public bool IsReply
        {
            get { return (Flags & ReplyMask) == ReplyMask; }
            set
            {
                if (value)
                    Flags |= ReplyMask;
                else
                    Flags &= ~ReplyMask;
            }
        }

        /// <summary>
        /// Group of commands.
        /// Only used in command packets.
        /// </summary>
        public int CommandSet
        {
            get { return GetByte(9); }
            set { SetByte(9, (byte)value); }
        }

        /// <summary>
        /// Actual command.
        /// Only used in command packets.
        /// </summary>
        public int Command
        {
            get { return GetByte(10); }
            set { SetByte(10, (byte)value); }
        }

        /// <summary>
        /// Error code. A value of 0 indicates success, other errors indicate an error.
        /// Only used in reply packets.
        /// </summary>
        public int ErrorCode
        {
            get { return GetInt16(9); }
            set { SetInt16(9, (byte)value); }
        }

        /// <summary>
        /// Is there an error?
        /// </summary>
        public bool HasError { get { return (ErrorCode != 0); } }

        /// <summary>
        /// Throw an exception if there is an error.
        /// </summary>
        public void ThrowOnError()
        {
            var code = ErrorCode;
            if (code == 0)
                return;
            throw new JdwpException(code);
        }

        /// <summary>
        /// Gets the data read/write accessor.
        /// </summary>
        public DataReaderWriter Data
        {
            get { return readerWriter ?? (readerWriter = CreateReaderWriter()); }
        }

        /// <summary>
        /// Create a reader/writer for the payload.
        /// </summary>
        protected virtual DataReaderWriter CreateReaderWriter()
        {
            return new DataReaderWriter(this, HeaderLength);
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            if (HasError)
                return string.Format("id={0}, error={1}", Id, ErrorCode);
            return string.Format("id={0}, cmdset={1}, cmd={2}", Id, CommandSet, Command);
        }

        /// <summary>
        /// Get a single byte from this packet.
        /// </summary>
        protected byte GetByte(int offset)
        {
            return data[dataOffset + offset];
        }

        /// <summary>
        /// Set a single byte in this packet.
        /// </summary>
        protected void SetByte(int offset, byte value)
        {
            data[dataOffset + offset] = value;
        }

        /// <summary>
        /// Get a 2-byte int from this packet.
        /// </summary>
        protected int GetInt16(int offset)
        {
            return converter.ToInt16(data, dataOffset + offset);
        }

        /// <summary>
        /// Set a 2-byte int in this packet.
        /// </summary>
        protected void SetInt16(int offset, int value)
        {
            var tmp = converter.GetBytes((short)value);
            Array.Copy(tmp, 0, data, dataOffset + offset, tmp.Length);
        }

        /// <summary>
        /// Get a 4-byte int from this packet.
        /// </summary>
        protected int GetInt32(int offset)
        {
            return converter.ToInt32(data, dataOffset + offset);
        }

        /// <summary>
        /// Get a 4-byte float from this packet.
        /// </summary>
        protected float GetFloat(int offset)
        {
            return converter.ToSingle(data, dataOffset + offset);
        }

        /// <summary>
        /// Set a 4-byte int in this packet.
        /// </summary>
        protected void SetInt32(int offset, int value)
        {
            var tmp = converter.GetBytes(value);
            Array.Copy(tmp, 0, data, dataOffset + offset, tmp.Length);
        }

        /// <summary>
        /// Get a 8-byte int from this packet.
        /// </summary>
        protected long GetInt64(int offset)
        {
            return converter.ToInt64(data, dataOffset + offset);
        }

        /// <summary>
        /// Get a 8-byte double from this packet.
        /// </summary>
        protected double GetDouble(int offset)
        {
            return converter.ToDouble(data, dataOffset + offset);
        }

        /// <summary>
        /// Set a 8-byte int in this packet.
        /// </summary>
        protected void SetInt64(int offset, long value)
        {
            var tmp = converter.GetBytes(value);
            Array.Copy(tmp, 0, data, dataOffset + offset, tmp.Length);
        }

        /// <summary>
        /// Get a 8-byte unsigned int from this packet.
        /// </summary>
        protected ulong GetUInt64(int offset)
        {
            return converter.ToUInt64(data, dataOffset + offset);
        }

        /// <summary>
        /// Set a 8-byte int in this packet.
        /// </summary>
        protected void SetUInt64(int offset, ulong value)
        {
            var tmp = converter.GetBytes(value);
            Array.Copy(tmp, 0, data, dataOffset + offset, tmp.Length);
        }

        /// <summary>
        /// Write this packet to the given stream.
        /// </summary>
        internal void WriteTo(Stream stream)
        {
            stream.Write(data, dataOffset, Length);
        }

        /// <summary>
        /// Provide sequential read/write the data part of the packet.
        /// </summary>
        public class DataReaderWriter
        {
            protected readonly JdwpPacket packet;
            protected int _offset;
            public int Offset { get { return _offset; } }

            /// <summary>
            /// Default ctor
            /// </summary>
            internal DataReaderWriter(JdwpPacket packet, int offset)
            {
                this.packet = packet;
                this._offset = offset;
            }

            /// <summary>
            /// Gets the ID size information
            /// </summary>
            public IdSizeInfo IdSizeInfo { get { return packet.serverInfo.IdSizeInfo; } }

            

            /// <summary>
            /// Gets a byte from the current offset relative to the start of the data part.
            /// </summary>
            public byte GetByte()
            {
                var x = _offset;
                _offset += 1;
                return packet.GetByte(x);
            }

            /// <summary>
            /// Sets a byte at the current offset relative to the start of the data part.
            /// </summary>
            public void SetByte(byte value)
            {
                packet.SetByte(_offset, value);
                _offset += 1;
            }

            /// <summary>
            /// Gets a boolean from the current offset relative to the start of the data part.
            /// </summary>
            public bool GetBoolean()
            {
                return (GetByte() != 0);
            }

            /// <summary>
            /// Sets a boolean at the current offset relative to the start of the data part.
            /// </summary>
            public void SetBoolean(bool value)
            {
                SetByte((byte)(value ? 1 : 0));
            }

            /// <summary>
            /// Gets an 16-bit int from the current offset relative to the start of the data part.
            /// </summary>
            public int GetInt16()
            {
                var x = _offset;
                _offset += 2;
                return packet.GetInt16(x);
            }

            /// <summary>
            /// Sets an 16-bit int at the current offset relative to the start of the data part.
            /// </summary>
            public void SetInt16(int value)
            {
                packet.SetInt16(_offset, value);
                _offset += 2;
            }

            /// <summary>
            /// Gets an int from the current offset relative to the start of the data part.
            /// </summary>
            public int GetInt()
            {
                var x = _offset;
                _offset += 4;
                return packet.GetInt32(x);
            }

            /// <summary>
            /// Sets an int at the current offset relative to the start of the data part.
            /// </summary>
            public void SetInt(int value)
            {
                packet.SetInt32(_offset, value);
                _offset += 4;
            }

            /// <summary>
            /// Gets an float from the current offset relative to the start of the data part.
            /// </summary>
            public float GetFloat()
            {
                var x = _offset;
                _offset += 4;
                return packet.GetFloat(x);
            }

            /// <summary>
            /// Gets a long from the current offset relative to the start of the data part.
            /// </summary>
            public long GetLong()
            {
                var x = _offset;
                _offset += 8;
                return packet.GetInt64(x);
            }

            /// <summary>
            /// Sets a long at the current offset relative to the start of the data part.
            /// </summary>
            public void SetLong(long value)
            {
                packet.SetInt64(_offset, value);
                _offset += 8;
            }

            /// <summary>
            /// Gets a double from the current offset relative to the start of the data part.
            /// </summary>
            public double GetDouble()
            {
                var x = _offset;
                _offset += 8;
                return packet.GetDouble(x);
            }

            /// <summary>
            /// Gets a ulong from the current offset relative to the start of the data part.
            /// </summary>
            public ulong GetULong()
            {
                var x = _offset;
                _offset += 8;
                return packet.GetUInt64(x);
            }

            /// <summary>
            /// Sets a long at the current offset relative to the start of the data part.
            /// </summary>
            public void SetULong(ulong value)
            {
                packet.SetUInt64(_offset, value);
                _offset += 8;
            }

            /// <summary>
            /// Gets a string from the current offset relative to the start of the data part.
            /// </summary>
            public string GetString()
            {
                var len = GetInt();
                if (len == 0)
                    return string.Empty;
                var result = Encoding.UTF8.GetString(packet.data, packet.dataOffset + _offset, len);
                _offset += len;
                return result;
            }

            /// <summary>
            /// Sets a string at the current offset relative to the start of the data part.
            /// </summary>
            public void SetString(string value)
            {
                var encoded = Encoding.UTF8.GetBytes(value);
                SetInt(encoded.Length);
                Array.Copy(encoded, 0, packet.data, packet.dataOffset + _offset, encoded.Length);
                _offset += encoded.Length;
            }

            /// <summary>
            /// Gets the size in bytes that the given string will occupy in this payload.
            /// </summary>
            public static int GetStringSize(string value)
            {
                var encoded = Encoding.UTF8.GetBytes(value);
                return 4 + encoded.Length;
            }            

            /// <summary>
            /// Advance the current offset with a given number of bytes.
            /// </summary>
            public void Skip(int count)
            {
                _offset += count;
            }
        }
    }
}
