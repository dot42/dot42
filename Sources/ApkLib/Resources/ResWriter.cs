using System;
using System.IO;

namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// Helper used to write resources.
    /// </summary>
    public sealed class ResWriter : IDisposable
    {
        private readonly Stream stream;
        [ThreadStatic]
        private static byte[] writeBuffer;

        /// <summary>
        /// Little endian ctor
        /// </summary>
        public ResWriter(Stream stream)
            : this(stream, false)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResWriter(Stream stream, bool bigEndian)
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
        /// Gets the underlying stream
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
        }

        /// <summary>
        /// Gets the current position in the stream
        /// </summary>
        public int Position
        {
            get { return (int) stream.Position; }
        }

        /// <summary>
        /// Byte order
        /// </summary>
        public bool BigEndian { get; private set; }

        /// <summary>
        /// Write a single byte
        /// </summary>
        public void WriteByte(int value)
        {
            stream.WriteByte((byte) value);
        }

        /// <summary>
        /// Write a single uint16
        /// </summary>
        public void WriteUInt16(int value)
        {
            WriteInt(value, 2);
        }

        /// <summary>
        /// Write a single uint32
        /// </summary>
        public void WriteInt32(int value)
        {
            WriteInt(value, 4);
        }

        /// <summary>
        /// Write a single uint16 to the given buffer at the given offset.
        /// This does not change the stream at all.
        /// </summary>
        public void WriteUInt16(byte[] buffer, int offset, int value)
        {
            EncodeInt(buffer, offset, value, 2);
        }

        /// <summary>
        /// Write a single int32 to the given buffer at the given offset.
        /// This does not change the stream at all.
        /// </summary>
        public void WriteInt32(byte[] buffer, int offset, int value)
        {
            EncodeInt(buffer, offset, value, 4);
        }

        /// <summary>
        /// Read an integer array
        /// </summary>
        public void WriteIntArray(int[] array)
        {
            WriteIntArray(array, 0, array.Length);
        }

        /// <summary>
        /// Read an integer array
        /// </summary>
        public void WriteIntArray(int[] array, int offset, int length)
        {
            while (length > 0)
            {
                WriteInt32(array[offset++]);
                length--;
            }
        }

        /// <summary>
        /// Write the entire given byte array 
        /// </summary>
        public void WriteByteArray(byte[] array)
        {
            WriteByteArray(array, 0, array.Length);
        }
        /// <summary>
        /// Write a byte array of given length
        /// </summary>
        public void WriteByteArray(byte[] array, int offset, int length)
        {
            stream.Write(array, offset, length);
        }


        /// <summary>
        /// Read a string containing a fixed number of UInt16 characters.
        /// </summary>
        public void WriteFixedLenghtUnicodeString(string value, int length)
        {
            value = value ?? string.Empty;
            for (var i = 0; i < length; i++)
            {
                var ch = (i < value.Length) ? value[i] : 0;
                WriteUInt16(ch);
            }
        }

        /// <summary>
        /// Fill x bytes with '0'
        /// </summary>
        public void Fill(int bytes)
        {
            if (bytes <= 0)
            {
                return;
            }
            while (bytes > 0)
            {
                stream.WriteByte(0);
                bytes--;
            }
        }

        /// <summary>
        /// Add '0' to end of a x byte alignment
        /// </summary>
        public void PadAlign(int alignment)
        {
            while ((stream.Position & (alignment-1)) != 0)
            {
                stream.WriteByte(0);
            }
        }

        /// <summary>
        /// Create an unsigned 16-int mark
        /// </summary>
        internal Mark.UInt16 MarkUInt16()
        {
            return new Mark.UInt16(this);
        }

        /// <summary>
        /// Create an 32-int mark
        /// </summary>
        internal Mark.Int32 MarkInt32()
        {
            return new Mark.Int32(this);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        private void WriteInt(int value, int length)
        {
            if (length < 0 || length > 4)
            {
                throw new ArgumentException("length");
            }
            if (writeBuffer == null)
            {
                writeBuffer = new byte[4];
            }
            EncodeInt(writeBuffer, 0, value, length);
            stream.Write(writeBuffer, 0, length);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        private void EncodeInt(byte[] buffer, int offset, int value, int length)
        {
            EncodeInt(buffer, offset, value, length, BigEndian);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        internal static void EncodeInt(byte[] buffer, int offset, int value, int length, bool bigEndian)
        {
            if (length < 0 || length > 4)
            {
                throw new ArgumentException("length");
            }
            if (bigEndian)
            {
                for (var i = length - 1; i >= 0; i--)
                {
                    buffer[offset + i] = (byte)(value & 0xFF);
                    value = value >> 8;
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    buffer[offset + i] = (byte)(value & 0xFF);
                    value = value >> 8;
                }
            }
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        internal static void EncodeInt16(byte[] buffer, int offset, int value, bool bigEndian)
        {
            EncodeInt(buffer, offset, value, 2, bigEndian);
        }

        /// <summary>
        /// Number reading helper
        /// </summary>
        internal static void EncodeInt32(byte[] buffer, int offset, int value, bool bigEndian)
        {
            EncodeInt(buffer, offset, value, 4, bigEndian);
        }
    }
}
