using System;
using System.IO;
using System.Text;

namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// Helper used to read resources.
    /// </summary>
    public sealed class ResReader : IDisposable
    {
        private readonly Stream stream;
        [ThreadStatic]
        private static byte[] readBuffer;

        /// <summary>
        /// Little endian ctor
        /// </summary>
        public ResReader(Stream stream)
            : this(stream, false)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResReader(Stream stream, bool bigEndian)
        {
            this.stream = stream;
            BigEndian = bigEndian;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
        }

        /// <summary>
        /// Byte order
        /// </summary>
        public bool BigEndian { get; private set; }

        /// <summary>
        /// Read an upcoming chunk type.
        /// Restore state of stream afterwards.
        /// </summary>
        public ChunkTypes PeekChunkType()
        {
            var position = stream.Position;
            var result = (ChunkTypes) ReadUInt16();
            stream.Position = position;
            return result;
        }

        /// <summary>
        /// Read a single uint8
        /// </summary>
        public int ReadByte()
        {
            var result = stream.ReadByte();
            if (result < 0)
                throw new EndOfStreamException();
            return result;
        }

        /// <summary>
        /// Read a single uint16
        /// </summary>
        public int ReadUInt16()
        {
            return ReadInt(2);
        }

        /// <summary>
        /// Read a single uint32
        /// </summary>
        public int ReadInt32()
        {
            return ReadInt(4);
        }

        /// <summary>
        /// Read a single uint16 from the given buffer at the given offset.
        /// This does not change the stream at all.
        /// </summary>
        public int ReadUInt16(byte[] buffer, int offset)
        {
            return DecodeInt(buffer, offset, 2);
        }

        /// <summary>
        /// Read a single int32 from the given buffer at the given offset.
        /// This does not change the stream at all.
        /// </summary>
        public int ReadInt32(byte[] buffer, int offset)
        {
            return DecodeInt(buffer, offset, 4);
        }

        /// <summary>
        /// Read an integer array of given length
        /// </summary>
        public int[] ReadIntArray(int length)
        {
            var array = new int[length];
            ReadIntArray(array, 0, length);
            return array;
        }

        /// <summary>
        /// Read an integer array
        /// </summary>
        public void ReadIntArray(int[] array, int offset, int length)
        {
            while (length > 0)
            {
                array[offset++] = ReadInt32();
                length--;
            }
        }

        /// <summary>
        /// Read a byte array of given length
        /// </summary>
        public byte[] ReadByteArray(int length)
        {
            var array = new byte[length];
            var read = stream.Read(array, 0, length);
            if (read != length)
            {
                throw new EndOfStreamException();
            }
            return array;
        }

        /// <summary>
        /// Read a string containing a fixed number of UInt16 characters.
        /// </summary>
        public string ReadFixedLenghtUnicodeString(int length)
        {
            var sb = new StringBuilder();
            var ended = false;
            for (var i = 0; i < length; i++)
            {
                var ch = (char)ReadUInt16();
                ended |= (ch == 0);
                if (!ended)
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Skip x bytes
        /// </summary>
        public void Skip(int bytes)
        {
            if (bytes <= 0)
            {
                return;
            }
            if (stream.CanSeek)
            {
                stream.Seek(bytes, SeekOrigin.Current);
            }
            else
            {
                while (bytes > 0)
                {
                    if (stream.ReadByte() < 0)
                        throw new EndOfStreamException();
                    bytes--;
                }
            }
        }

        /// <summary>
        /// Skip 4 bytes
        /// </summary>
        public void SkipInt()
        {
            Skip(4);
        }

        /// <summary>
        /// Gets the number of bytes left in the stream
        /// </summary>
        public int Available
        {
            get { return (int) (stream.Length - stream.Position); }
        }

        /// <summary>
        /// Gets the current position in the stream
        /// </summary>
        public int Position
        {
            get { return (int) stream.Position; }
            set { stream.Position = value; }
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        private int ReadInt(int length)
        {
            if (length < 0 || length > 4)
            {
                throw new ArgumentException("length");
            }
            if (readBuffer == null)
            {
                readBuffer = new byte[4];
            }
            stream.Read(readBuffer, 0, length);
            return DecodeInt(readBuffer, 0, length);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        private int DecodeInt(byte[] buffer, int offset, int length)
        {
            return DecodeInt(buffer, offset, length, BigEndian);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        internal static int DecodeInt(byte[] buffer, int offset, int length, bool bigEndian)
        {
            if (length < 0 || length > 4)
            {
                throw new ArgumentException("length");
            }
            var result = 0;
            if (bigEndian)
            {
                for (var i = (length - 1) * 8; i >= 0; i -= 8)
                {
                    var b = buffer[offset++];
                    result |= (b << i);
                }
            }
            else
            {
                length *= 8;
                for (var i = 0; i != length; i += 8)
                {
                    var b = buffer[offset++];
                    result |= (b << i);
                }
            }
            return result;
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        internal static int DecodeInt16(byte[] buffer, int offset, bool bigEndian)
        {
            return DecodeInt(buffer, offset, 2, bigEndian);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        internal static int DecodeInt32(byte[] buffer, int offset, bool bigEndian)
        {
            return DecodeInt(buffer, offset, 4, bigEndian);
        }
    }
}
