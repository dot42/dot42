using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dot42.ApkLib.Resources
{
    public class StringPool : Chunk, IEnumerable<string>
    {
        private readonly List<Entry> list = new List<Entry>();
        private readonly int[] styleOffsets;
        private readonly int[] styles;

        
        /// <summary>
        /// Creation ctor
        /// </summary>
        internal StringPool()
            : base(ChunkTypes.RES_STRING_POOL_TYPE)
        {            
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        internal StringPool(ResReader reader)
            : base(reader, ChunkTypes.RES_STRING_POOL_TYPE)
        {
            var stringCount = reader.ReadInt32();
            var styleOffsetCount = reader.ReadInt32();
            var flags = (StringPoolFlags) reader.ReadInt32();
            IsUtf8 = ((flags & StringPoolFlags.UTF8_FLAG) != 0);
            var stringsStart = reader.ReadInt32();
            var stylesStart = reader.ReadInt32();

            var stringOffsets = reader.ReadIntArray(stringCount);
            if (styleOffsetCount != 0)
            {
                styleOffsets = reader.ReadIntArray(styleOffsetCount);
            }
            byte[] stringData;
            {
                var size = ((stylesStart == 0) ? Size : stylesStart) - stringsStart;
                if ((size % 4) != 0)
                {
                    throw new IOException(string.Format("String data size is not multiple of 4 ({0}).", size));
                }
                stringData = reader.ReadByteArray(size);
            }
            if (stylesStart != 0)
            {
                var size = (Size - stylesStart);
                if ((size % 4) != 0)
                {
                    throw new IOException(string.Format("Style data size is not multiple of 4 ({0}).", size));
                }
                styles = reader.ReadIntArray(size / 4);
            }

            var count = (stringOffsets != null) ? stringOffsets.Length : 0;
            for (var i = 0; i != count; ++i)
            {
                list.Add(new Entry(GetRaw(i, stringOffsets, stringData, IsUtf8, reader), -1));
            }
        }

        /// <summary>
        /// Gets the string at the given index 
        /// </summary>
        public string this[int index]
        {
            get
            {
                if ((index < 0) || (index >= list.Count))
                    return null;
                return list[index].Item1;
            }
        }

        /// <summary>
        /// Returns number of strings in block. 
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        /// Get the index of the given string.
        /// Returns the index of the string or -1 if not found.
        /// </summary>
        public int IndexOf(string value, int resourceId)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if ((e.ResourceId == resourceId) && (e.Value == value))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Add a given string.
        /// Returns the index of the string.
        /// </summary>
        public int Add(string value, int resourceId)
        {
            var index = IndexOf(value, resourceId);
            if (index < 0)
            {
                index = list.Count;
                list.Add(new Entry(value, resourceId));
            }
            return index;
        }

        /// <summary>
        /// Returns the index of the string.
        /// If the string does not exist, an exception is thrown.
        /// </summary>
        public int Get(string value, int resourceId)
        {
            var index = IndexOf(value, resourceId);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Unknown string: " + value);
            }
            return index;
        }

        /// <summary>
        /// Are strings encodes as UTF8?
        /// </summary>
        public bool IsUtf8 { get; private set; }

        /// <summary>
        /// Sort for resource ID and value
        /// </summary>
        public void Sort()
        {
            list.Sort();
        }

        /// <summary>
        /// Gets all entries
        /// </summary>
        internal IEnumerable<Entry> Entries { get { return list; } }

        /// <summary>
        /// Returns raw string (without any styling information) at specified index.
        /// Returns null if index is invalid or object was not initialized.
        /// </summary>
        private static string GetRaw(int index, int[] stringOffsets, byte[] strings, bool isUtf8, ResReader reader)
        {
            if (index < 0 || (stringOffsets == null) || (index >= stringOffsets.Length))
            {
                return null;
            }            
            var offset = stringOffsets[index];
            int length;

            if (isUtf8)
            {
                // charsize = 1
                // Decode length
                var u16Length = DecodeLength8(strings, ref offset);
                length = DecodeLength8(strings, ref offset);
                return AndroidEncodings.UTF8.GetString(strings, offset, length);
            }

            // charsize = 2
            length = DecodeLength16(reader, strings, ref offset);
            var data = new char[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = (char) reader.ReadUInt16(strings, offset);
                offset += 2;
            }
            return new string(data);
        }

        /// <summary>
        /// Default a length for charsize=1
        /// </summary>
        private static int DecodeLength8(byte[] strings, ref int offset)
        {
            var length = (int)strings[offset++];
            if ((length & 0x80) != 0)
            {
                length = ((length & 0x7F) << 8) | strings[offset++];
            }
            return length;
        }

        /// <summary>
        /// Default a length for charsize=2
        /// </summary>
        private static int DecodeLength16(ResReader reader, byte[] strings, ref int offset)
        {
            var length = reader.ReadUInt16(strings, offset);
            offset += 2;
            if ((length & 0x8000) != 0)
            {
                length = ((length & 0x7FFF) << 16) | reader.ReadUInt16(strings, offset);
                offset += 2;
            }
            return length;
        }

        /// <summary>
        /// Returns string with style tags (html-like). 
        /// </summary>
        public string GetHTML(int index)
        {
            var raw = this[index];
            if (raw == null)
            {
                return null;
            }
            var style = GetStyle(index);
            if (style == null)
            {
                return raw;
            }
            var html = new StringBuilder(raw.Length + 32);
            var offset = 0;
            while (true)
            {
                int i = -1;
                for (int j = 0; j != style.Length; j += 3)
                {
                    if (style[j + 1] == -1)
                    {
                        continue;
                    }
                    if (i == -1 || style[i + 1] > style[j + 1])
                    {
                        i = j;
                    }
                }
                var start = ((i != -1) ? style[i + 1] : raw.Length);
                for (var j = 0; j != style.Length; j += 3)
                {
                    var end = style[j + 2];
                    if (end == -1 || end >= start)
                    {
                        continue;
                    }
                    if (offset <= end)
                    {
                        html.Append(raw, offset, end + 1);
                        offset = end + 1;
                    }
                    style[j + 2] = -1;
                    html.Append('<');
                    html.Append('/');
                    html.Append(this[style[j]]);
                    html.Append('>');
                }
                if (offset < start)
                {
                    html.Append(raw, offset, start);
                    offset = start;
                }
                if (i == -1)
                {
                    break;
                }
                html.Append('<');
                html.Append(this[style[i]]);
                html.Append('>');
                style[i + 1] = -1;
            }
            return html.ToString();
        }

        /// <summary>
        /// Returns style information - array of int triplets,
        /// where in each triplet:
        /// 	* first int is index of tag name ('b','i', etc.)
        ///  	* second int is tag start index in string
        ///  	* third int is tag end index in string
        /// </summary>
        private int[] GetStyle(int index)
        {
            if (styleOffsets == null || styles == null ||
                index >= styleOffsets.Length)
            {
                return null;
            }
            int offset = styleOffsets[index]/4;
            int[] style;
            {
                int count = 0;
                for (int i = offset; i < styles.Length; ++i)
                {
                    if (styles[i] == -1)
                    {
                        break;
                    }
                    count += 1;
                }
                if (count == 0 || (count%3) != 0)
                {
                    return null;
                }
                style = new int[count];
            }
            for (int i = offset, j = 0; i < styles.Length;)
            {
                if (styles[i] == -1)
                {
                    break;
                }
                style[j++] = styles[i++];
            }
            return style;
        }

        /// <summary>
        /// Write the header of this chunk.
        /// Always call the base method first.
        /// </summary>
        protected override void WriteHeader(ResWriter writer)
        {
            base.WriteHeader(writer); // header
            writer.WriteInt32(Count); // stringCount
            writer.WriteInt32(0); // styleCount
            var flags = (StringPoolFlags) 0;
            if (IsUtf8) flags |= StringPoolFlags.UTF8_FLAG;
            writer.WriteInt32((int) flags); // flags
            writer.WriteInt32(28 + Count * 4);  // stringsStart
            writer.WriteInt32(0); // stylesStart
        }

        /// <summary>
        /// Write the data of this chunk.
        /// </summary>
        protected override void WriteData(ResWriter writer)
        {
            byte[] data;
            int[] offsets;
            BuildStringsData(list, IsUtf8, writer, out offsets, out data);
            writer.WriteIntArray(offsets);
            writer.WriteByteArray(data);
        }

        /// <summary>
        /// Encode a string data array.
        /// </summary>
        private static void BuildStringsData(List<Entry> list, bool isUtf8, ResWriter writer, out int[] offsets, out byte[] data)
        {
            // Build offsets
            offsets = new int[list.Count];

            // Build data
            var dataStream = new MemoryStream();
            var dataWriter = new ResWriter(dataStream, writer.BigEndian);
            for (var i = 0; i < list.Count; i++)
            {
                var s = list[i].Value;
                offsets[i] = (int) dataStream.Position;
                if (isUtf8)
                {
                    EncodeLength8(dataWriter, s.Length); // u16 length
                    var encoded = AndroidEncodings.UTF8.GetBytes(s);
                    EncodeLength8(dataWriter, encoded.Length);
                    dataStream.Write(encoded, 0, encoded.Length);
                }
                else
                {
                    EncodeLength16(dataWriter, s.Length);
                    foreach (var ch in s)
                    {
                        dataWriter.WriteUInt16(ch);
                    }
                    dataWriter.WriteUInt16(0); // Pad with '\0'
                }
            }

            // Pad with '0' to end on 4 byte boundary
            dataWriter.PadAlign(4);

            // Return actual data
            data = dataStream.ToArray();
        }

        /// <summary>
        /// Encode a length in charsize=1 mode
        /// </summary>
        private static void EncodeLength8(ResWriter writer, int length)
        {
            if (length > 0x7F)
            {
                // use 2 bytes
                writer.WriteByte(0x80 | ((length >> 8) & 0x7F));
            }
            writer.WriteByte(length & 0x7F);
        }

        /// <summary>
        /// Encode a length in charsize=3 mode
        /// </summary>
        private static void EncodeLength16(ResWriter writer, int length)
        {
            if (length > 0x7FFF)
            {
                // use 2 chars
                writer.WriteUInt16(0x8000 | ((length >> 16) & 0x7FFF));
            }
            writer.WriteUInt16(length & 0x7FFF);
        }

        /// <summary>
        /// Single entry
        /// </summary>
        public class Entry : Tuple<string, int>, IComparable<Entry> 
        {
            public Entry(string value, int resourceId)
                : base(value, resourceId)
            {
            }

            public string Value { get { return Item1; } }
            public int ResourceId { get { return Item2; } }

            public bool HasResourceId { get { return (ResourceId != -1); } }

            /// <summary>
            /// Compares the current object with another object of the same type.
            /// </summary>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public int CompareTo(Entry other)
            {
                if (ResourceId == other.ResourceId)
                {
                    return string.CompareOrdinal(Value, other.Value);
                }
                if (ResourceId < 0)
                    return 1;
                if (other.ResourceId < 0)
                    return -1;
                return (ResourceId < other.ResourceId) ? -1 : 1;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<string> GetEnumerator()
        {
            return list.Select(x => x.Value).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
