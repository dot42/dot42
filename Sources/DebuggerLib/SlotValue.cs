using System;

namespace Dot42.DebuggerLib
{
    public class SlotValue
    {
        public readonly int Slot;
        public readonly object Value;
        private readonly Jdwp.Tag Tag;

        public SlotValue(int slot, object value) : this(slot, DetermineTag(value), value)
        {
        }

        public SlotValue(int slot, Jdwp.Tag tag, object value)
        {
            Slot = slot;
            Value = value;
            Tag = tag;
        }



        private static Jdwp.Tag DetermineTag(object value)
        {
            if (value == null)
                return Jdwp.Tag.Object;
            if (value is int || value is uint)
                return Jdwp.Tag.Int;
            if (value is short)
                return Jdwp.Tag.Short;
            if (value is byte || value is sbyte)
                return Jdwp.Tag.Short;
            if (value is char || value is ushort)
                return Jdwp.Tag.Char;
            if (value is float)
                return Jdwp.Tag.Float;
            if (value is double)
                return Jdwp.Tag.Double;
            if (value is string)
                return Jdwp.Tag.String;
            if (value.GetType().IsArray)
                return Jdwp.Tag.Array;
            return Jdwp.Tag.Object;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", Slot, Value);
        }

        /// <summary>
        /// writes slot and value.
        /// </summary>
        /// <param name="data"></param>
        public void Write(JdwpPacket.DataReaderWriter data)
        {
            data.SetInt(Slot);
            WriteValue(data);
        }

        /// <summary>
        /// writes only the value, without the slot.
        /// </summary>
        /// <param name="data"></param>
        public void WriteValue(JdwpPacket.DataReaderWriter data)
        {
            data.SetByte((byte)Tag);
            WriteValue(data, Tag, Value);
        }

        /// <summary>
        /// Read an untagged value.
        /// </summary>
        private static void WriteValue(JdwpPacket.DataReaderWriter data, Jdwp.Tag tag, object obj)
        {
            switch (tag)
            {
                //case Jdwp.Tag.Array:
                //case Jdwp.Tag.Object:
                //case Jdwp.Tag.String:
                //case Jdwp.Tag.Thread:
                //case Jdwp.Tag.ThreadGroup:
                //case Jdwp.Tag.ClassLoader:
                //case Jdwp.Tag.ClassObject:
                //    return new ObjectId(readerWriter);
                case Jdwp.Tag.Byte:
                    data.SetByte((byte)obj);
                    break;
                case Jdwp.Tag.Short:
                case Jdwp.Tag.Char:
                    data.SetInt16((int) obj);
                    break;
                //case Jdwp.Tag.Float:
                //    readerWriter.GetFloat(); //?
                //    break;
                //case Jdwp.Tag.Double:
                //    return readerWriter.SetDouble(); //?
                case Jdwp.Tag.Int:
                    data.SetInt((int)obj);
                    break;
                case Jdwp.Tag.Long:
                    data.SetLong((long) obj);
                    break;
                case Jdwp.Tag.Boolean:
                    data.SetBoolean((bool)obj);
                    break;
                default:
                    throw new ArgumentException("unsupported tag " + tag);
            }
        }
    }
}
