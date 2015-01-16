using System;
using System.IO;
using System.Text;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// JVM specific extension methods used to read class files.
    /// </summary>
    internal static class StreamExtensions
    {
        /// <summary>
        /// Read an unsigned 8-bit int.
        /// </summary>
        internal static byte ReadU1(this Stream reader)
        {
            var x0 = reader.ReadByte() & 0xFF;
            return (byte) x0;
        }

        /// <summary>
        /// Read an unsigned 16-bit int.
        /// </summary>
        internal static int ReadU2(this Stream reader)
        {
            var x1 = reader.ReadByte() & 0xFF;
            var x0 = reader.ReadByte() & 0xFF;
            return (x1 << 8) | x0;
        }

        /// <summary>
        /// Read an unsigned 32-bit int.
        /// </summary>
        internal static uint ReadU4(this Stream reader)
        {
            var x3 = (uint) (reader.ReadByte() & 0xFF);
            var x2 = (uint)(reader.ReadByte() & 0xFF);
            var x1 = (uint)(reader.ReadByte() & 0xFF);
            var x0 = (uint)(reader.ReadByte() & 0xFF);
            return (x3 << 24) | (x2 << 16) | (x1 << 8) | x0;
        }

        /// <summary>
        /// Read an signed 32-bit int.
        /// </summary>
        internal static int ReadS4(this Stream reader)
        {
            return (int) reader.ReadU4();
        }

        /// <summary>
        /// Read an signed 64-bit int.
        /// </summary>
        internal static long ReadS8(this Stream reader)
        {
            var h = (long)reader.ReadU4();
            var l = (long)reader.ReadU4();
            return (h << 32) | l;
        }

        /// <summary>
        /// Read an 32-bit float.
        /// </summary>
        internal static float ReadF4(this Stream reader)
        {
            var bits = reader.ReadU4();
            var bytes = BitConverter.GetBytes(bits);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Read an 64-bit float.
        /// </summary>
        internal static double ReadF8(this Stream reader)
        {
            var bits = (ulong)reader.ReadS8();
            var bytes = BitConverter.GetBytes(bits);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Read an UTF8 byte array of length bytes.
        /// </summary>
        internal static string ReadUTF8(this Stream reader, int length)
        {
            var bytes = new byte[length];
            reader.Read(bytes, 0, length);

            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var x = bytes[i];
                if ((x & 0x80) == 0)
                {
                    // Single byte
                    sb.Append((char) x);
                    continue;
                }

                var y = bytes[++i];
                if ((x >> 5) == 6)
                {
                    // 2 bytes
                    var ch = ((x & 0x1F) << 6) + (y & 0x3F);
                    sb.Append((char) ch);
                    continue;
                }

                var z = bytes[++i];
                {
                    var ch = ((x & 0xF) << 12) + ((y & 0x3F) << 6) + (z & 0x3F);
                    sb.Append((char) ch);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Skip a number of bytes
        /// </summary>
        internal static void Skip(this Stream stream, long length)
        {
            if (stream.CanSeek)
            {
                stream.Seek(length, SeekOrigin.Current);
            }
            else
            {
                while (length > 0)
                {
                    stream.ReadByte();
                    length--;
                }
            }
        }
    }
}
