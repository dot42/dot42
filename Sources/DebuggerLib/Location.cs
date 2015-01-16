using System;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// An executable location
    /// </summary>
    public sealed class Location : IEquatable<Location>
    {
        public readonly ReferenceTypeId Class;
        public readonly MethodId Method;
        public readonly ulong Index;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Location(ReferenceTypeId classId, MethodId methodId, ulong index)
        {
            Class = classId;
            Method = methodId;
            Index = index;
        }

        /// <summary>
        /// Read an value.
        /// </summary>
        public Location(JdwpPacket.DataReaderWriter readerWriter)
        {
            Class = ReferenceTypeId.Read(readerWriter);
            Method = new MethodId(readerWriter);
            Index = readerWriter.GetULong();
        }
        
        /// <summary>
        /// Size of this location in bytes.
        /// </summary>
        internal int Size { get { return 1 + Class.Size + Method.Size + 8; } }

        /// <summary>
        /// Write this location into the given packet writer.
        /// </summary>
        public void WriteTo(JdwpPacket.DataReaderWriter writer)
        {
            writer.SetByte((byte) GetType(Class));
            Class.WriteTo(writer);
            Method.WriteTo(writer);
            writer.SetULong(Index);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Location other)
        {
            return (other != null) && Class.Equals(other.Class) && Method.Equals(other.Method) && (Index == other.Index);
        }

        public override string ToString()
        {
            return Class + "." + Method + " (" + Index + ")";
        }

        /// <summary>
        /// Does this location refer to NULL?
        /// </summary>
        public bool IsNull { get { return Class.IsNull; } }

        /// <summary>
        /// Gets the type for the given reference id.
        /// </summary>
        private static Jdwp.TypeTag GetType(ReferenceTypeId classId)
        {
            if (classId is ClassId) return Jdwp.TypeTag.Class;
            if (classId is InterfaceId) return Jdwp.TypeTag.Interface;
            if (classId is ArrayTypeId) return Jdwp.TypeTag.Array;
            throw new ArgumentException("Unknown classid " + classId);
        }
    }
}
