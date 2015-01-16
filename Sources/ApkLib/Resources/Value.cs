using System;
using System.Diagnostics;

namespace Dot42.ApkLib.Resources
{
    /// <summary>
    /// Res_value
    /// </summary>
    [DebuggerDisplay("{@Type},{@Data}")]
    public sealed class Value
    {
        public enum Types
        {
            // Contains no data.
            TYPE_NULL = 0x00,
            // The 'data' holds a ResTable_ref, a reference to another resource
            // table entry.
            TYPE_REFERENCE = 0x01,
            // The 'data' holds an attribute resource identifier.
            TYPE_ATTRIBUTE = 0x02,
            // The 'data' holds an index into the containing resource table's
            // global value string pool.
            TYPE_STRING = 0x03,
            // The 'data' holds a single-precision floating point number.
            TYPE_FLOAT = 0x04,
            // The 'data' holds a complex number encoding a dimension value,
            // such as "100in".
            TYPE_DIMENSION = 0x05,
            // The 'data' holds a complex number encoding a fraction of a
            // container.
            TYPE_FRACTION = 0x06,

            // Beginning of integer flavors...
            TYPE_FIRST_INT = 0x10,

            // The 'data' is a raw integer value of the form n..n.
            TYPE_INT_DEC = 0x10,
            // The 'data' is a raw integer value of the form 0xn..n.
            TYPE_INT_HEX = 0x11,
            // The 'data' is either 0 or 1, for input "false" or "true" respectively.
            TYPE_INT_BOOLEAN = 0x12,

            // Beginning of color integer flavors...
            TYPE_FIRST_COLOR_INT = 0x1c,

            // The 'data' is a raw integer value of the form #aarrggbb.
            TYPE_INT_COLOR_ARGB8 = 0x1c,
            // The 'data' is a raw integer value of the form #rrggbb.
            TYPE_INT_COLOR_RGB8 = 0x1d,
            // The 'data' is a raw integer value of the form #argb.
            TYPE_INT_COLOR_ARGB4 = 0x1e,
            // The 'data' is a raw integer value of the form #rgb.
            TYPE_INT_COLOR_RGB4 = 0x1f,

            // ...end of integer flavors.
            TYPE_LAST_COLOR_INT = 0x1f,

            // ...end of integer flavors.
            TYPE_LAST_INT = 0x1f
        }

        private readonly Types type;
        private readonly int data;

        /// <summary>
        /// Creation ctor
        /// </summary>
        public Value(Types type, int data)
        {
            this.type = type;
            this.data = data;
        }

        /// <summary>
        /// Reading ctor
        /// </summary>
        internal Value(ResReader reader)
        {
            var size = reader.ReadUInt16();
            reader.Skip(1); // res0
            type = (Types)reader.ReadByte();
            data = reader.ReadInt32();
        }
        
        /// <summary>
        /// Write this chunk.
        /// </summary>
        public void Write(ResWriter writer)
        {
            var size = 8;
            writer.WriteUInt16(size);
            writer.Fill(1); // res0
            writer.WriteByte((int) type);
            writer.WriteInt32(data);
        }

        /// <summary>
        /// Gets the actual data
        /// </summary>
        public int Data
        {
            get { return data; }
        }

        /// <summary>
        /// Gets the type of value
        /// </summary>
        public Types Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the data as string representation of the typed data.
        /// </summary>
        public string GetValue(XmlTree tree)
        {
            switch (type)
            {
                    // Contains no data.
                case Types.TYPE_NULL:
                    return null;
         
                    // The 'data' holds a ResTable_ref, a reference to another resource
                    // table entry.
                case Types.TYPE_REFERENCE:
                    return data.ToString();

                    // The 'data' holds an attribute resource identifier.
                case Types.TYPE_ATTRIBUTE:
                    
                    // The 'data' holds an index into the containing resource table's
                    // global value string pool.
                case Types.TYPE_STRING:
                    return tree.StringPool[data];

                    // The 'data' holds a single-precision floating point number.
                case Types.TYPE_FLOAT:
                        return BitConverter.ToSingle(BitConverter.GetBytes(data), 0).ToString();

                    // The 'data' holds a complex number encoding a dimension value,
                    // such as "100in".
                case Types.TYPE_DIMENSION:

                    // The 'data' holds a complex number encoding a fraction of a
                    // container.
                case Types.TYPE_FRACTION:

                    // The 'data' is a raw integer value of the form n..n.
                case Types.TYPE_INT_DEC:
                    return data.ToString();
                    // The 'data' is a raw integer value of the form 0xn..n.
                case Types.TYPE_INT_HEX:
                    return "0x" + data.ToString("X");
                    // The 'data' is either 0 or 1, for input "false" or "true" respectively.
                case Types.TYPE_INT_BOOLEAN:
                    return (data == 0) ? "false" : "true";

                    // The 'data' is a raw integer value of the form #aarrggbb.
                case Types.TYPE_INT_COLOR_ARGB8:
                    return string.Format("#{0:X8}", data);
                    // The 'data' is a raw integer value of the form #rrggbb.
                case Types.TYPE_INT_COLOR_RGB8:
                    return string.Format("#{0:X6}", data);
                    // The 'data' is a raw integer value of the form #argb.
                case Types.TYPE_INT_COLOR_ARGB4:
                    {
                        var all = data.ToString("X8");
                        return "#" + all[0] + all[2] + all[4] + all[6];
                    }
                    // The 'data' is a raw integer value of the form #rgb.
                case Types.TYPE_INT_COLOR_RGB4:
                    {
                        var all = data.ToString("X6");
                        return "#" + all[0] + all[2] + all[4];
                    }

                default:
                    return string.Format("unknown type {0}", (int) type);
            }
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Data: {1:X8}", Type, Data);
        }
    }
}
