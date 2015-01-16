using System;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Some type of object reference.
    /// </summary>
    public abstract class Id<T> : IEquatable<Id<T>>
        where T : Id<T>
    {
        private readonly int idSize;
        private readonly long id;

        /// <summary>
        /// Read an value.
        /// </summary>
        protected Id(JdwpPacket.DataReaderWriter readerWriter, int idSize)
        {
            this.idSize = idSize;
            switch (idSize)
            {
                case 4:
                    id = readerWriter.GetInt();
                    break;
                case 8:
                    id = readerWriter.GetLong();
                    break;
                default:
                    throw new ArgumentException("Unsupported ID size " + idSize);
            }
        }

        /// <summary>
        /// Null.
        /// </summary>
        protected Id(int idSize)
        {
            this.idSize = idSize;
            id = 0;
        }

        /// <summary>
        /// Size of this id in bytes.
        /// </summary>
        internal int Size { get { return idSize; } }

        /// <summary>
        /// Write this ID into the given packet writer.
        /// </summary>
        public void WriteTo(JdwpPacket.DataReaderWriter readerWriter)
        {
            switch (idSize)
            {
                case 4:
                    readerWriter.SetInt((int) id);
                    break;
                case 8:
                    readerWriter.SetLong(id);
                    break;
                default:
                    throw new ArgumentException("Unsupported ID size " + idSize);
            }            
        }

        /// <summary>
        /// Does this id reference to NULL?
        /// </summary>
        public bool IsNull { get { return id == 0; } }

        /// <summary>
        /// Is this equal to other?
        /// </summary>
        public override bool Equals(object other)
        {
            return Equals(other as Id<T>);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return (int) (id & 0xFFFFFFFF);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Id<T> other)
        {
            return (other != null) && (other.id == id);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
            return id.ToString();
        }
    }

    /// <summary>
    /// Some type of object reference.
    /// </summary>
    public abstract class ObjectId<T> : Id<T>
        where T : ObjectId<T>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        protected ObjectId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter, readerWriter.IdSizeInfo.ObjectIdSize)
        {
        }

        /// <summary>
        /// Null.
        /// </summary>
        protected ObjectId(IdSizeInfo sizeInfo)
            : base(sizeInfo.ObjectIdSize)
        {
        }

        /// <summary>
        /// Null.
        /// </summary>
        protected ObjectId(int size)
            : base(size)
        {
        }

        /// <summary>
        /// Read an value.
        /// </summary>
        protected ObjectId(JdwpPacket.DataReaderWriter readerWriter, int size)
            : base(readerWriter, size)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies an object in the target VM.
    /// </summary>
    public sealed class ObjectId : ObjectId<ObjectId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public ObjectId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a reference type in the target VM. It should not be assumed that for a particular class, the classObjectID and the 
    /// referenceTypeID are the same. A particular reference type will be identified by exactly one ID in JDWP commands and replies 
    /// throughout its lifetime A referenceTypeID is not reused to identify a different reference type, regardless of whether the referenced class has been unloaded.
    /// </summary>
    public abstract class ReferenceTypeId : ObjectId<ReferenceTypeId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        protected ReferenceTypeId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter, readerWriter.IdSizeInfo.ReferenceTypeIdSize)
        {
        }

        /// <summary>
        /// Null.
        /// </summary>
        protected ReferenceTypeId(IdSizeInfo sizeInfo)
            : base(sizeInfo.ReferenceTypeIdSize)
        {
        }

        /// <summary>
        /// Read a (byte) TypeTag followed by a reference type id.
        /// </summary>
        internal static ReferenceTypeId Read(JdwpPacket.DataReaderWriter reader)
        {
            var tag = (Jdwp.TypeTag)reader.GetByte();
            switch (tag)
            {
                case 0:
                    var dummy = new ClassId(reader);
                    return new ClassId(reader.IdSizeInfo);
                case Jdwp.TypeTag.Class:
                    return new ClassId(reader);
                case Jdwp.TypeTag.Interface:
                    return new InterfaceId(reader);
                case Jdwp.TypeTag.Array:
                    return new ArrayTypeId(reader);
                default:
                    throw new ArgumentException("Unknown type tag " + (int)tag);
            }
        }
    }

    /// <summary>
    /// Uniquely identifies a thread in the target VM.
    /// </summary>
    public sealed class ThreadId : ObjectId<ThreadId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public ThreadId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter)
        {
        }

        /// <summary>
        /// Read an value.
        /// </summary>
        public ThreadId(JdwpPacket.DataReaderWriter readerWriter, int size)
            : base(readerWriter, size)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a thread group in the target VM.
    /// </summary>
    public sealed class ThreadGroupId : ObjectId<ThreadGroupId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public ThreadGroupId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a stack frame in the target VM.
    /// </summary>
    public sealed class FrameId : Id<FrameId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public FrameId(JdwpPacket.DataReaderWriter readerWriter, IdSizeInfo sizeInfo)
            : base(readerWriter, sizeInfo.FrameIdSize)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a reference type in the target VM that is known to be a class type.
    /// </summary>
    public sealed class ClassId : ReferenceTypeId
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public ClassId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter)
        {
        }

        /// <summary>
        /// Null.
        /// </summary>
        public ClassId(IdSizeInfo sizeInfo)
            : base(sizeInfo)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a reference type in the target VM that is known to be a interface type.
    /// </summary>
    public sealed class InterfaceId : ReferenceTypeId
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public InterfaceId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a reference type in the target VM that is known to be a array type.
    /// </summary>
    public sealed class ArrayTypeId : ReferenceTypeId
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public ArrayTypeId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a method in some class in the target VM. The methodID must uniquely identify the method within its class/interface or
    /// any of its subclasses/subinterfaces/implementors. A methodID is not necessarily unique on its own; it is always paired with a referenceTypeID 
    /// to uniquely identify one method. The referenceTypeID can identify either the declaring type of the method or a subtype.
    /// </summary>
    public sealed class MethodId : Id<MethodId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public MethodId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter, readerWriter.IdSizeInfo.MethodIdSize)
        {
        }
    }

    /// <summary>
    /// Uniquely identifies a field in some class in the target VM. The fieldID must uniquely identify the field within its class/interface or any 
    /// of its subclasses/subinterfaces/implementors. A fieldID is not necessarily unique on its own; it is always paired with a referenceTypeID to 
    /// uniquely identify one field. The referenceTypeID can identify either the declaring type of the field or a subtype.
    /// </summary>
    public sealed class FieldId : Id<FieldId>
    {
        /// <summary>
        /// Read an value.
        /// </summary>
        public FieldId(JdwpPacket.DataReaderWriter readerWriter)
            : base(readerWriter, readerWriter.IdSizeInfo.FieldIdSize)
        {
        }
    }
}
