namespace Dot42.DebuggerLib.Events.Jdwp
{
    /// <summary>
    /// Notification of an exception in the target VM. If the exception is thrown from a non-native method, the exception event is generated 
    /// at the location where the exception is thrown. If the exception is thrown from a native method, the exception event is generated at 
    /// the first non-native location reached after the exception is thrown. 
    /// </summary>
    public sealed class Exception : BaseLocationEvent
    {
        private readonly TaggedObjectId exception;
        private readonly Location catchLocation;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Exception(JdwpPacket.DataReaderWriter reader)
            : base(reader)
        {
            exception = new TaggedObjectId(reader);
            catchLocation = new Location(reader);
        }

        public TaggedObjectId ExceptionObject
        {
            get { return exception; }
        }

        public Location CatchLocation
        {
            get { return catchLocation; }
        }

        /// <summary>
        /// Does this event describe a (user) caught exception?
        /// If not, it is a thrown exception.
        /// </summary>
        public bool IsCaught
        {
            get { return !catchLocation.IsNull; }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TResult Accept<TResult, TData>(EventVisitor<TResult, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}
