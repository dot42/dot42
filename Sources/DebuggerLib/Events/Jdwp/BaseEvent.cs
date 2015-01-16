using System;

namespace Dot42.DebuggerLib.Events.Jdwp
{
    public abstract class BaseEvent : JdwpEvent
    {
        private readonly int requestId;
        private readonly ThreadId threadId;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected BaseEvent(JdwpPacket.DataReaderWriter reader)
        {
            requestId = reader.GetInt();
            threadId = new ThreadId(reader);
        }

        /// <summary>
        /// Request that generated event (or 0 if this event is automatically generated. 
        /// </summary>
        public int RequestId
        {
            get { return requestId; }
        }

        /// <summary>
        /// Involved thread 
        /// </summary>
        public ThreadId ThreadId
        {
            get { return threadId; }
        }
    }

    /// <summary>
    /// Base event + location
    /// </summary>
    public abstract class BaseLocationEvent : BaseEvent
    {
        private readonly Location location;

        /// <summary>
        /// Default ctor. 
        /// </summary>
        protected BaseLocationEvent(JdwpPacket.DataReaderWriter reader)
            : base(reader)
        {
            location = new Location(reader);
        }

        /// <summary>
        /// Location stepped to 
        /// </summary>
        public Location Location
        {
            get { return location; }
        }
    }

    public abstract class BaseFieldEvent : BaseLocationEvent
    {
        private readonly DebuggerLib.Jdwp.TypeTag refTypeTag;
        private readonly ReferenceTypeId typeId;
        private readonly FieldId fieldId;
        private readonly TaggedObjectId objectId;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected BaseFieldEvent(JdwpPacket.DataReaderWriter reader)
            : base(reader)
        {
            refTypeTag = (DebuggerLib.Jdwp.TypeTag)reader.GetByte();
            switch (refTypeTag)
            {
                case DebuggerLib.Jdwp.TypeTag.Class:
                    typeId = new ClassId(reader);
                    break;
                case DebuggerLib.Jdwp.TypeTag.Interface:
                    typeId = new InterfaceId(reader);
                    break;
                case DebuggerLib.Jdwp.TypeTag.Array:
                    typeId = new ArrayTypeId(reader);
                    break;
                default:
                    throw new ArgumentException("Unknown type tag " + (int)refTypeTag);
            }
            fieldId = new FieldId(reader);
            objectId = new TaggedObjectId(reader);
        }

        public ReferenceTypeId TypeId
        {
            get { return typeId; }
        }

        public FieldId FieldId
        {
            get { return fieldId; }
        }

        public TaggedObjectId ObjectId
        {
            get { return objectId; }
        }
    }
}
