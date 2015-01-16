using System;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// DDMS specific packet type that is in the payload of a normal JDWP packet.
    /// </summary>
    public sealed class Chunk : JdwpPacket
    {
        internal const int ChunkHeaderLength = 8;

        internal const int DdmsCommandSet = 0xC7;
        internal const int DdmsCommand = 0x01;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Chunk(IJdwpServerInfo serverInfo, byte[] data, int dataOffset)
            : base(serverInfo, data, dataOffset)
        {
            CommandSet = DdmsCommandSet;
            Command = DdmsCommand;
        }

        /// <summary>
        /// Chunk type
        /// </summary>
        public int Type
        {
            get { return GetInt32(HeaderLength); }
            set { SetInt32(HeaderLength, value); }
        }

        /// <summary>
        /// Length of chunk payload
        /// </summary>
        public new int Length
        {
            get { return GetInt32(HeaderLength + 4); }
            set
            {
                SetInt32(HeaderLength + 4, value);
                base.Length = HeaderLength + ChunkHeaderLength + value;
            }
        }

        /// <summary>
        /// Gets this packet as DDMS Chunk.
        /// </summary>
        internal override Chunk AsChunk()
        {
            return this;
        }

        /// <summary>
        /// Gets a string out of the payload.
        /// </summary>
        internal string GetString(int offset, int len)
        {
            var data = new char[len];
            for (var i = 0; i < len; i++)
            {
                data[i] = (char) GetInt16(offset);
                offset += 2;
            }
            return new string(data);
        }

        /// <summary>
        /// Sets a string int the payload.
        /// </summary>
        internal void SetString(int offset, string str)
        {
            var len = str.Length;
            for (int i = 0; i < len; i++)
            {
                SetInt16(offset, str[i]);
                offset += 2;
            }
        }

        /// <summary>
        /// Create a chunk.
        /// </summary>
        public static Chunk CreateChunk(IJdwpServerInfo serverInfo, int type, int dataLength, Action<Chunk> initialize = null)
        {
            var data = new byte[HeaderLength + ChunkHeaderLength + dataLength];
            var packet = new Chunk(serverInfo, data, 0) { Type = type, Length = dataLength };
            if (initialize != null)
            {
                initialize(packet);
            }
            return packet;
        }

        /// <summary>
        /// Convert a 4-character string to a 32-bit type.
        /// </summary>
        internal static int GetType(string typeName)
        {
            uint val = 0;

            if (typeName.Length != 4)
            {
                throw new ArgumentException("Type name must be 4 letter long");
            }
            for (var i = 0; i < 4; i++)
            {
                val <<= 8;
                val |= (byte)typeName[i];
            }
            return (int) val;
        }

        /// <summary>
        /// Convert an integer type to a 4-character string.
        /// </summary>
        internal static string GetTypeName(int type)
        {
            var ascii = new char[4];
            ascii[0] = (char)((type >> 24) & 0xff);
            ascii[1] = (char)((type >> 16) & 0xff);
            ascii[2] = (char)((type >> 8) & 0xff);
            ascii[3] = (char)(type & 0xff);
            return new string(ascii);
        }

        /// <summary>
        /// Gets payload accessor
        /// </summary>
        public new DataReaderWriter Data
        {
            get { return (DataReaderWriter) base.Data; }
        }

        /// <summary>
        /// Create a reader/writer for the payload.
        /// </summary>
        protected override JdwpPacket.DataReaderWriter CreateReaderWriter()
        {
            return new DataReaderWriter(this, HeaderLength + ChunkHeaderLength);
        }

        /// <summary>
        /// Convert to string.
        /// </summary>
        public override string ToString()
        {
            return string.Format("type={0}, length={1}", GetTypeName(Type), Length);
        }
        
        /// <summary>
        /// Provide sequential read/write the data part of the packet.
        /// </summary>
        public new class DataReaderWriter : JdwpPacket.DataReaderWriter
        {
            /// <summary>
            /// Default ctor
            /// </summary>
            internal DataReaderWriter(JdwpPacket packet, int offset)
                : base(packet, offset)
            {
            }

            /// <summary>
            /// Gets a string from the current offset relative to the start of the data part.
            /// </summary>
            public string GetString(int strLength)
            {
                var result = ((Chunk) packet).GetString(Offset, strLength);
                Offset += strLength * 2;
                return result;
            }

            /// <summary>
            /// Sets a string at the current offset relative to the start of the data part.
            /// </summary>
            public new void SetString(string value)
            {
                ((Chunk)packet).SetString(Offset, value);
                Offset += 2 * value.Length;
            }
        }
    }
}
