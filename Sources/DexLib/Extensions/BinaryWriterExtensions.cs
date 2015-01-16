using System;
using System.IO;
using Dot42.DexLib.IO.Markers;

namespace Dot42.DexLib.Extensions
{
    internal static class BinaryWriterExtensions
    {
        /// <summary>
        /// Ensure specific alignment from given section start offset.
        /// </summary>
        internal static void EnsureAlignment(this BinaryWriter writer, uint sectionOffset, int alignment)
        {
            var position = writer.BaseStream.Position /*- sectionOffset*/;
            while (position % alignment != 0)
            {
                writer.Write((byte)0);
                position++;
            }
        }

        public static void PreserveCurrentPosition(this BinaryWriter writer, uint newPosition, Action action)
        {
            long position = writer.BaseStream.Position;
            writer.BaseStream.Seek(newPosition, SeekOrigin.Begin);

            action();

            writer.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        internal static UShortMarker MarkUShort(this BinaryWriter writer)
        {
            return new UShortMarker(writer);
            ;
        }

        internal static UIntMarker MarkUInt(this BinaryWriter writer)
        {
            return new UIntMarker(writer);
            ;
        }

        internal static SizeOffsetMarker MarkSizeOffset(this BinaryWriter writer)
        {
            return new SizeOffsetMarker(writer);
        }

        internal static SignatureMarker MarkSignature(this BinaryWriter writer)
        {
            return new SignatureMarker(writer);
        }

        public static void WriteULEB128(this BinaryWriter writer, uint value)
        {
            byte partial;
            do
            {
                partial = (byte) (value & 0x7f);
                value >>= 7;
                if (value != 0)
                    partial |= 0x80;
                writer.Write(partial);
            } while (value != 0);
        }

        public static void WriteULEB128p1(this BinaryWriter writer, long value)
        {
            WriteULEB128(writer, (uint) (value + 1));
        }

        public static void WriteSLEB128(this BinaryWriter writer, int value)
        {
            bool negative = (value < 0);
            byte partial;
            bool next = true;

            while (next)
            {
                partial = (byte) (value & 0x7f);
                value >>= 7;
                if (negative)
                    value |= -(1 << 24);

                if ((value == 0 && ((partial & 0x40) == 0)) || (value == -1 && ((partial & 0x40) != 0)))
                    next = false;
                else
                    partial |= 0x80;

                writer.Write(partial);
            }
        }

        public static void WriteMUTF8String(this BinaryWriter writer, String value)
        {
            writer.WriteULEB128((uint) value.Length);

            // See dalvik\dx\src\com\android\dx\util\Mutf8.java
            for (int i = 0; i < value.Length; i++)
            {
                int c = value[i];
                if ((c != 0) && (c <= 127))
                {
                    writer.Write((byte) c);
                }
                else if (c <= 2047)
                {
                    writer.Write((byte) (0xc0 | (0x1f & (c >> 6))));
                    writer.Write((byte)(0x80 | (0x3f & c)));
                }
                else
                {
                    writer.Write((byte)(0xe0 | (0x0f & (c >> 12))));
                    writer.Write((byte)(0x80 | (0x3f & (c >> 6))));
                    writer.Write((byte)(0x80 | (0x3f & c)));
                }
            }

            writer.Write((byte) 0); // 0 padded;
        }

        private static int NumberOfLeadingZeros(long i)
        {
            if (i == 0)
                return 64;
            int n = 1;
            var x = (int) (TripleShift(i, 32));

            if (x == 0)
            {
                n += 32;
                x = (int) i;
            }
            if (TripleShift(x, 16) == 0)
            {
                n += 16;
                x <<= 16;
            }
            if (TripleShift(x, 24) == 0)
            {
                n += 8;
                x <<= 8;
            }
            if (TripleShift(x, 28) == 0)
            {
                n += 4;
                x <<= 4;
            }
            if (TripleShift(x, 30) == 0)
            {
                n += 2;
                x <<= 2;
            }

            n -= (int) TripleShift(x, 31);
            return n;
        }

        private static long TripleShift(long n, int s)
        {
            if (n >= 0)
                return n >> s;
            return (n >> s) + (2 << ~s);
        }

        public static int GetByteCountForSignedPackedNumber(this BinaryWriter writer, long value)
        {
            int requiredBits = 65 - NumberOfLeadingZeros(value ^ (value >> 63));
            int result = (byte) ((requiredBits + 0x07) >> 3);

            return result;
        }

        public static int GetByteCountForUnsignedPackedNumber(this BinaryWriter writer, long value)
        {
            int requiredBits = 64 - NumberOfLeadingZeros(value);
            if (requiredBits == 0)
                requiredBits = 1;

            int result = (byte) ((requiredBits + 0x07) >> 3);

            return result;
        }

        public static void WritePackedSignedNumber(this BinaryWriter writer, long value)
        {
            int requiredBytes = GetByteCountForSignedPackedNumber(writer, value);

            for (int i = 0; i < requiredBytes; i++)
            {
                writer.Write((byte) value);
                value >>= 8;
            }
        }

        public static void WriteUnsignedPackedNumber(this BinaryWriter writer, long value)
        {
            int requiredBytes = GetByteCountForUnsignedPackedNumber(writer, value);

            for (int i = 0; i < requiredBytes; i++)
            {
                writer.Write((byte) value);
                value >>= 8;
            }
        }
    }
}