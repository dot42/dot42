using System;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Object id with tag (type)
    /// </summary>
    public sealed class TaggedObjectId : IEquatable<TaggedObjectId>
    {
        public readonly Jdwp.Tag Tag;
        public readonly ObjectId Object;

        /// <summary>
        /// Read an value.
        /// </summary>
        public TaggedObjectId(JdwpPacket.DataReaderWriter readerWriter)
        {
            Tag = (Jdwp.Tag) readerWriter.GetByte();
            Object = new ObjectId(readerWriter);
        }
        
        /// <summary>
        /// Size of this location in bytes.
        /// </summary>
        internal int Size { get { return 1 + Object.Size; } }

        /// <summary>
        /// Write this location into the given packet writer.
        /// </summary>
        public void WriteTo(JdwpPacket.DataReaderWriter writer)
        {
            writer.SetByte((byte) Tag);
            Object.WriteTo(writer);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TaggedObjectId other)
        {
            return (other != null) && Object.Equals(other.Object) && (Tag == other.Tag);
        }

        public override string ToString()
        {
            return "(" + (char)Tag + ") " + Object;
        }
    }
}
