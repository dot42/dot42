using System;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Tag + value
    /// </summary>
    public sealed class Value 
    {
        public readonly Jdwp.Tag Tag;
        public readonly object ValueObject;

        /// <summary>
        /// Read a tagged value.
        /// </summary>
        public Value(JdwpPacket.DataReaderWriter readerWriter)
        {
            Tag = (Jdwp.Tag) readerWriter.GetByte();
            ValueObject = ReadUntaggedValue(readerWriter, Tag);
        }

        /// <summary>
        /// Read an untagged value.
        /// </summary>
        public Value(JdwpPacket.DataReaderWriter readerWriter, Jdwp.Tag tag)
        {
            Tag = tag;
            ValueObject = ReadUntaggedValue(readerWriter, tag);
        }

        /// <summary>
        /// Create from given source.
        /// </summary>
        public Value(TaggedObjectId source)
        {
            Tag = source.Tag;
            ValueObject = source.Object;
        }

        /// <summary>
        /// Is this a primivite value?
        /// If not it is an object reference.
        /// </summary>
        public bool IsPrimitive
        {
            get { return Tag.IsPrimitive(); }
        }

        /// <summary>
        /// Gets the value converted to string
        /// </summary>
        public string ValueAsString
        {
            get { return (ValueObject != null) ? ValueObject.ToString() : "<null>"; }
        }

        public override string ToString()
        {
            return "(" + (char)Tag + ") " + ValueObject;
        }

        /// <summary>
        /// Read an untagged value.
        /// </summary>
        private static object ReadUntaggedValue(JdwpPacket.DataReaderWriter readerWriter, Jdwp.Tag tag)
        {
            switch (tag)
            {
                case Jdwp.Tag.Array:
                case Jdwp.Tag.Object:
                case Jdwp.Tag.String:
                case Jdwp.Tag.Thread:
                case Jdwp.Tag.ThreadGroup:
                case Jdwp.Tag.ClassLoader:
                case Jdwp.Tag.ClassObject:
                    return new ObjectId(readerWriter);
                case Jdwp.Tag.Byte:
                    return readerWriter.GetByte();
                case Jdwp.Tag.Char:
                    return (char)readerWriter.GetInt16();
                case Jdwp.Tag.Float:
                    return readerWriter.GetFloat(); 
                case Jdwp.Tag.Double:
                    return readerWriter.GetDouble();
                case Jdwp.Tag.Int:
                    return readerWriter.GetInt();
                case Jdwp.Tag.Long:
                    return readerWriter.GetLong();
                case Jdwp.Tag.Short:
                    return readerWriter.GetInt16();
                case Jdwp.Tag.Void:
                    return null;
                case Jdwp.Tag.Boolean:
                    return readerWriter.GetBoolean();
                default:
                    throw new ArgumentException("Unknown tag " + (int)tag);
            }
        }
    }
}
